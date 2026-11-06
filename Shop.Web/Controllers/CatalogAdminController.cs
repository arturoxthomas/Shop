using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;
using Shop.Web.Data.Catalog.Entities;
using Shop.Web.Models;

namespace Shop.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/catalog")]
public sealed class CatalogAdminController : Controller
{
    private readonly ShopCatalogContext db;
    public CatalogAdminController(ShopCatalogContext db) => this.db = db;

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() =>
        View(await db.ShopCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync());

    [HttpGet("categories/new")]
    public IActionResult NewCategory() => View("CategoryForm", new CatalogAdminForm());

    [HttpGet("categories/{id:int}/edit")]
    public async Task<IActionResult> EditCategory(int id)
    {
        var c = await db.ShopCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? NotFound() : View("CategoryForm", new CatalogAdminForm
        {
            Id = c.Id, Name = c.Name, Slug = c.Slug, Description = c.Description, IsActive = c.IsActive
        });
    }

    [HttpPost("categories/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CatalogAdminForm form)
    {
        form.Name = form.Name?.Trim() ?? "";
        form.Slug = form.Slug?.Trim() ?? "";
        form.Description = form.Description?.Trim();
        if (await db.ShopCategories.AnyAsync(c => c.Slug == form.Slug && c.Id != form.Id))
            ModelState.AddModelError(nameof(form.Slug), "Ya existe una categoría con este identificador.");
        if (!ModelState.IsValid) return View("CategoryForm", form);

        if (form.Id == 0)
        {
            db.ShopCategories.Add(new ShopCategory
            {
                Name = form.Name, Slug = form.Slug, Description = form.Description, IsActive = form.IsActive
            });
        }
        else
        {
            var category = await db.ShopCategories.FindAsync(form.Id);
            if (category == null) return NotFound();
            category.Name = form.Name;
            category.Slug = form.Slug;
            category.Description = form.Description;
            category.IsActive = form.IsActive;
        }
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo guardar la categoría. Comprueba que el identificador sea único.");
            return View("CategoryForm", form);
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("brands")]
    public async Task<IActionResult> Brands() =>
        View(await db.ShopBrands.AsNoTracking().OrderBy(b => b.Name).ToListAsync());

    [HttpGet("brands/new")]
    public IActionResult NewBrand() => View("BrandForm", new CatalogAdminForm());

    [HttpGet("brands/{id:int}/edit")]
    public async Task<IActionResult> EditBrand(int id)
    {
        var b = await db.ShopBrands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return b == null ? NotFound() : View("BrandForm", new CatalogAdminForm
        {
            Id = b.Id, Name = b.Name, Slug = b.Slug, IsActive = b.IsActive
        });
    }

    [HttpPost("brands/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBrand(CatalogAdminForm form)
    {
        form.Name = form.Name?.Trim() ?? "";
        form.Slug = form.Slug?.Trim() ?? "";
        // Description is not stored for brands.
        ModelState.Remove(nameof(form.Description));
        if (await db.ShopBrands.AnyAsync(b => b.Slug == form.Slug && b.Id != form.Id))
            ModelState.AddModelError(nameof(form.Slug), "Ya existe una marca con este identificador.");
        if (!ModelState.IsValid) return View("BrandForm", form);

        if (form.Id == 0)
        {
            db.ShopBrands.Add(new ShopBrand { Name = form.Name, Slug = form.Slug, IsActive = form.IsActive });
        }
        else
        {
            var brand = await db.ShopBrands.FindAsync(form.Id);
            if (brand == null) return NotFound();
            brand.Name = form.Name;
            brand.Slug = form.Slug;
            brand.IsActive = form.IsActive;
        }
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo guardar la marca. Comprueba que el identificador sea único.");
            return View("BrandForm", form);
        }
        return RedirectToAction(nameof(Brands));
    }
}
