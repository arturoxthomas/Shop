using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;
using Shop.Web.Services;

namespace Shop.Web.Controllers;

[Route("cart")]
public sealed class CartController : Controller
{
    private readonly ShopCatalogContext db;
    private readonly ShopCartService cart;
    public CartController(ShopCatalogContext db, ShopCartService cart)
    {
        this.db = db;
        this.cart = cart;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var saved = cart.Read();
        var ids = saved.Select(i => i.ProductId).ToList();
        var products = await db.ShopCatalogProducts.AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.IsPublished && p.Category.IsActive)
            .Select(p => new { p.Id, p.Name, p.Price, p.Stock }).ToListAsync();
        var lines = saved.Join(products, s => s.ProductId, p => p.Id,
            (s, p) => new CartDisplayLine(p.Id, p.Name, p.Price, p.Stock, s.Quantity)).ToList();
        return View(new CartDisplayModel(lines));
    }

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        if (quantity is < 1 or > 99) return BadRequest();
        var product = await db.ShopCatalogProducts.AsNoTracking()
            .Where(p => p.Id == productId && p.IsPublished && p.Category.IsActive)
            .Select(p => new { p.Id, p.Stock }).FirstOrDefaultAsync();
        if (product == null) return NotFound();
        var items = cart.Read();
        var current = items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;
        if (current + quantity > 99 || product.Stock < current + quantity)
        {
            TempData["CartError"] = "La cantidad solicitada supera las existencias disponibles.";
            return RedirectToAction("Details", "Catalog", new { id = productId });
        }
        if (current == 0 && items.Count >= 50)
        {
            TempData["CartError"] = "El carrito admite un máximo de 50 productos diferentes.";
            return RedirectToAction(nameof(Index));
        }
        items.RemoveAll(i => i.ProductId == productId);
        items.Add(new ShopCartItem(productId, current + quantity));
        cart.Save(items);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        if (quantity is < 0 or > 99) return BadRequest();
        var items = cart.Read();
        if (items.All(i => i.ProductId != productId)) return RedirectToAction(nameof(Index));
        if (quantity > 0)
        {
            var stock = await db.ShopCatalogProducts.AsNoTracking()
                .Where(p => p.Id == productId && p.IsPublished && p.Category.IsActive)
                .Select(p => (decimal?)p.Stock).FirstOrDefaultAsync();
            if (stock == null || stock < quantity)
            {
                TempData["CartError"] = "No hay suficientes existencias para esa cantidad.";
                return RedirectToAction(nameof(Index));
            }
        }
        items.RemoveAll(i => i.ProductId == productId);
        if (quantity > 0) items.Add(new ShopCartItem(productId, quantity));
        cart.Save(items);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        cart.Save(Array.Empty<ShopCartItem>());
        return RedirectToAction(nameof(Index));
    }
}

public sealed record CartDisplayLine(int ProductId, string Name, decimal Price, decimal Stock, int Quantity)
{
    public decimal Subtotal => Price * Quantity;
    public bool HasEnoughStock => Stock >= Quantity;
}
public sealed record CartDisplayModel(IReadOnlyList<CartDisplayLine> Lines)
{
    public decimal Total => Lines.Sum(i => i.Subtotal);
    public int ItemCount => Lines.Sum(i => i.Quantity);
}
