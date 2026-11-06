using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;
using Shop.Web.Data.Catalog.Entities;
using Shop.Web.Models;

namespace Shop.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/catalog/products")]
public sealed class CatalogProductsAdminController : Controller
{
    private readonly ShopCatalogContext db;
    public CatalogProductsAdminController(ShopCatalogContext db) => this.db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(await db.ShopCatalogProducts.AsNoTracking()
            .Include(p => p.Category).Include(p => p.Brand)
            .OrderBy(p => p.Name).Take(200).ToListAsync());

    [HttpGet("new")]
    public async Task<IActionResult> Create()
    {
        await PopulateOptions();
        return View("Form", new CatalogProductForm());
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await db.ShopCatalogProducts.AsNoTracking().Include(p => p.ShopProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        await PopulateOptions();
        return View("Form", new CatalogProductForm {
            Id = p.Id, Sku = p.Sku, Name = p.Name, Slug = p.Slug,
            Description = p.Description, CategoryId = p.CategoryId, BrandId = p.BrandId,
            Price = p.Price, Stock = p.Stock, IsPublished = p.IsPublished,
            ImageUrl = p.ShopProductImages.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
        });
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CatalogProductForm form)
    {
        form.Sku = form.Sku?.Trim() ?? "";
        form.Name = form.Name?.Trim() ?? "";
        form.Slug = form.Slug?.Trim() ?? "";
        form.Description = form.Description?.Trim();
        form.ImageUrl = form.ImageUrl?.Trim();
        if (string.IsNullOrEmpty(form.ImageUrl)) form.ImageUrl = null;

        if (!await db.ShopCategories.AnyAsync(c => c.Id == form.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(form.CategoryId), "Selecciona una categoría activa.");
        if (form.BrandId.HasValue && !await db.ShopBrands.AnyAsync(b => b.Id == form.BrandId && b.IsActive))
            ModelState.AddModelError(nameof(form.BrandId), "Selecciona una marca activa.");
        if (await db.ShopCatalogProducts.AnyAsync(p => p.Id != form.Id && (p.Sku == form.Sku || p.Slug == form.Slug)))
            ModelState.AddModelError("", "El SKU o identificador URL ya está registrado.");
        if (form.ImageUrl != null && !Uri.TryCreate(form.ImageUrl, UriKind.Absolute, out var imageUri))
            ModelState.AddModelError(nameof(form.ImageUrl), "La URL de imagen no es válida.");
        else if (form.ImageUrl != null && imageUri!.Scheme != Uri.UriSchemeHttps)
            ModelState.AddModelError(nameof(form.ImageUrl), "La imagen debe usar HTTPS.");
        if (!ModelState.IsValid) { await PopulateOptions(); return View("Form", form); }

        ShopCatalogProduct p;
        if (form.Id == 0)
        {
            p = new ShopCatalogProduct { CreatedAtUtc = DateTime.UtcNow };
            db.ShopCatalogProducts.Add(p);
        }
        else
        {
            p = await db.ShopCatalogProducts.Include(x => x.ShopProductImages)
                .FirstOrDefaultAsync(x => x.Id == form.Id) ?? null!;
            if (p == null) return NotFound();
        }
        p.Sku = form.Sku; p.Name = form.Name; p.Slug = form.Slug;
        p.Description = form.Description; p.CategoryId = form.CategoryId;
        p.BrandId = form.BrandId; p.Price = form.Price; p.Stock = form.Stock;
        p.IsPublished = form.IsPublished;

        var primary = p.ShopProductImages.OrderBy(i => i.SortOrder).FirstOrDefault();
        if (form.ImageUrl == null)
        {
            if (primary != null) db.ShopProductImages.Remove(primary);
        }
        else if (primary == null)
            p.ShopProductImages.Add(new ShopProductImage { ImageUrl = form.ImageUrl, SortOrder = 0 });
        else
            primary.ImageUrl = form.ImageUrl;

        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo guardar. Verifica los datos y valores únicos.");
            await PopulateOptions();
            return View("Form", form);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptions()
    {
        ViewBag.Categories = new SelectList(await db.ShopCategories.AsNoTracking()
            .Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.Brands = new SelectList(await db.ShopBrands.AsNoTracking()
            .Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
    }
}
