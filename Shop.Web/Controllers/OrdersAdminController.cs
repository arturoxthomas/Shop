using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Orders;

namespace Shop.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/orders")]
public sealed class OrdersAdminController : Controller
{
    private readonly ShopOrdersContext db;
    private static readonly string[] Statuses = { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
    public OrdersAdminController(ShopOrdersContext db) => this.db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(await db.Orders.AsNoTracking().OrderByDescending(o => o.Id).Take(100).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost("{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(int id, string status)
    {
        if (!Statuses.Contains(status)) return BadRequest();
        var order = await db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        // Cancelling and restoring stock require a dedicated inventory workflow.
        if (status == "Cancelled" || order.Status == "Cancelled") return BadRequest("La cancelación requiere reposición controlada de inventario.");
        order.Status = status;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }
}
