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
    private readonly IWebHostEnvironment environment;
    public CatalogProductsAdminController(ShopCatalogContext db, IWebHostEnvironment environment)
    {
        this.db = db;
        this.environment = environment;
    }

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
            ExistingImages = p.ShopProductImages.OrderBy(i => i.SortOrder).Select(i => new CatalogImageItem(i.Id, i.ImageUrl)).ToList()
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
        var existing = form.Id == 0 ? null : await db.ShopCatalogProducts
            .Include(p => p.ShopProductImages).FirstOrDefaultAsync(p => p.Id == form.Id);
        if (form.Id != 0 && existing == null) return NotFound();
        var existingImages = existing?.ShopProductImages.ToList() ?? new List<ShopProductImage>();
        var removed = form.RemoveImageIds.Distinct().ToHashSet();
        if (removed.Any(id => existingImages.All(i => i.Id != id)))
            ModelState.AddModelError("", "La imagen seleccionada no pertenece a este producto.");
        if (existingImages.Count(i => !removed.Contains(i.Id)) + form.Images.Count > 5)
            ModelState.AddModelError(nameof(form.Images), "Máximo cinco imágenes por producto.");
        if (!await db.ShopCategories.AnyAsync(c => c.Id == form.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(form.CategoryId), "Selecciona una categoría activa.");
        if (form.BrandId.HasValue && !await db.ShopBrands.AnyAsync(b => b.Id == form.BrandId && b.IsActive))
            ModelState.AddModelError(nameof(form.BrandId), "Selecciona una marca activa.");
        if (await db.ShopCatalogProducts.AnyAsync(p => p.Id != form.Id && (p.Sku == form.Sku || p.Slug == form.Slug)))
            ModelState.AddModelError("", "El SKU o identificador URL ya está registrado.");
        foreach (var file in form.Images)
            if (file.Length is <= 0 or > 5242880 || !await IsValidImage(file))
                ModelState.AddModelError(nameof(form.Images), "Cada imagen debe ser JPG, PNG o WebP válida, máximo 5 MB.");
        form.ExistingImages = existingImages.OrderBy(i => i.SortOrder)
            .Select(i => new CatalogImageItem(i.Id, i.ImageUrl)).ToList();
        if (!ModelState.IsValid) { await PopulateOptions(); return View("Form", form); }

        var p = existing ?? new ShopCatalogProduct { CreatedAtUtc = DateTime.UtcNow };
        if (existing == null) db.ShopCatalogProducts.Add(p);
        p.Sku = form.Sku; p.Name = form.Name; p.Slug = form.Slug;
        p.Description = form.Description; p.CategoryId = form.CategoryId;
        p.BrandId = form.BrandId; p.Price = form.Price; p.Stock = form.Stock;
        p.IsPublished = form.IsPublished;
        foreach (var image in existingImages.Where(i => removed.Contains(i.Id)))
            db.ShopProductImages.Remove(image);

        var writtenFiles = new List<string>();
        try
        {
            var folder = Path.Combine(environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(folder);
            var order = existingImages.Where(i => !removed.Contains(i.Id))
                .Select(i => i.SortOrder).DefaultIfEmpty(-1).Max() + 1;
            foreach (var file in form.Images)
            {
                var ext = await ImageExtension(file);
                var filename = Guid.NewGuid().ToString("N") + ext;
                var diskPath = Path.Combine(folder, filename);
                await using (var stream = new FileStream(diskPath, FileMode.CreateNew))
                    await file.CopyToAsync(stream);
                writtenFiles.Add(diskPath);
                p.ShopProductImages.Add(new ShopProductImage {
                    ImageUrl = "/uploads/products/" + filename, AltText = p.Name, SortOrder = order++
                });
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DbUpdateException)
        {
            foreach (var file in writtenFiles)
                if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
            ModelState.AddModelError("", "No fue posible guardar el producto o sus imágenes.");
            await PopulateOptions();
            return View("Form", form);
        }
        foreach (var image in existingImages.Where(i => removed.Contains(i.Id)))
        {
            const string prefix = "/uploads/products/";
            if (!image.ImageUrl.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var filename = image.ImageUrl[prefix.Length..];
            if (Path.GetFileName(filename) != filename) continue;
            var diskPath = Path.Combine(environment.WebRootPath, "uploads", "products", filename);
            try { if (System.IO.File.Exists(diskPath)) System.IO.File.Delete(diskPath); }
            catch (IOException) { /* Cleanup can be retried later. */ }
        }
        return RedirectToAction(nameof(Index));
    }

    private static async Task<string?> ImageExtension(IFormFile file)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var n = await stream.ReadAsync(header);
        if (n >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255) return ".jpg";
        if (n >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
        if (n >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP") return ".webp";
        return null;
    }

    private static async Task<bool> IsValidImage(IFormFile file) =>
        file.ContentType is ("image/jpeg" or "image/png" or "image/webp") &&
        await ImageExtension(file) != null;

    private async Task PopulateOptions()
    {
        ViewBag.Categories = new SelectList(await db.ShopCategories.AsNoTracking()
            .Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.Brands = new SelectList(await db.ShopBrands.AsNoTracking()
            .Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
    }
}
