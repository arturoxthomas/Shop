using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;
using Shop.Web.Data.Orders;
using Shop.Web.Models;
using Shop.Web.Services;

namespace Shop.Web.Controllers;

[Route("checkout")]
public sealed class CheckoutController : Controller
{
    private readonly ShopCatalogContext catalog;
    private readonly ShopOrdersContext orders;
    private readonly ShopCartService cart;
    public CheckoutController(ShopCatalogContext catalog, ShopOrdersContext orders, ShopCartService cart)
    {
        this.catalog = catalog;
        this.orders = orders;
        this.cart = cart;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        if (cart.Read().Count == 0) return RedirectToAction("Index", "Cart");
        return View(new CheckoutForm());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutForm form)
    {
        if (!ModelState.IsValid) return View("Index", form);
        var items = cart.Read();
        if (items.Count == 0) return RedirectToAction("Index", "Cart");
        if (items.Count > 50 || items.Any(x => x.Quantity is < 1 or > 99))
            return BadRequest();
        var ids = items.Select(x => x.ProductId).ToList();
        var products = await catalog.ShopCatalogProducts.AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.IsPublished && p.Category.IsActive)
            .Select(p => new { p.Id, p.Sku, p.Name, p.Price }).ToListAsync();
        if (products.Count != items.Count)
        {
            ModelState.AddModelError("", "Algunos productos ya no están disponibles. Revisa tu carrito.");
            return View("Index", form);
        }

        await using var transaction = await orders.Database.BeginTransactionAsync();
        try
        {
            // Atomic conditional updates prevent overselling under concurrent checkouts.
            foreach (var item in items.OrderBy(x => x.ProductId))
            {
                var affected = await orders.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE dbo.ShopCatalogProducts SET Stock = Stock - {item.Quantity}
                       WHERE Id = {item.ProductId} AND Stock >= {item.Quantity}
                       AND IsPublished = 1
                       AND EXISTS (SELECT 1 FROM dbo.ShopCategories c
                                   WHERE c.Id = ShopCatalogProducts.CategoryId AND c.IsActive = 1)");
                if (affected != 1)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "No hay suficientes existencias. Revisa el carrito.");
                    return View("Index", form);
                }
            }
            var order = new ShopOrder
            {
                CustomerName = form.CustomerName.Trim(),
                CustomerEmail = form.CustomerEmail.Trim(),
                Phone = form.Phone?.Trim(),
                ShippingAddress = form.ShippingAddress.Trim(),
                Notes = form.Notes?.Trim(),
                Status = "Pending",
                CreatedAtUtc = DateTime.UtcNow,
                Items = items.Select(i =>
                {
                    var p = products.Single(p => p.Id == i.ProductId);
                    return new ShopOrderItem {
                        ProductId = p.Id, ProductName = p.Name, Sku = p.Sku,
                        Quantity = i.Quantity, UnitPrice = p.Price
                    };
                }).ToList()
            };
            order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            orders.Orders.Add(order);
            await orders.SaveChangesAsync();
            await transaction.CommitAsync();
            cart.Save(Array.Empty<ShopCartItem>());
            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "No se pudo registrar el pedido. Intenta nuevamente.");
            return View("Index", form);
        }
    }

    [HttpGet("confirmation/{id:int}")]
    public IActionResult Confirmation(int id)
    {
        // No personal order details on this public, guessable URL.
        return View(model: id);
    }
}
