using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;

namespace Shop.Web.Controllers;

[Route("catalog")]
public sealed class CatalogController : Controller
{
    private readonly ShopCatalogContext db;
    public CatalogController(ShopCatalogContext db) => this.db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        page = Math.Clamp(page, 1, 100000);
        search = search?.Trim();
        if (search?.Length > 100) search = search[..100];

        var query = db.ShopCatalogProducts.AsNoTracking()
            .Where(p => p.IsPublished && p.Category.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));

        if (categoryId is > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var total = await query.CountAsync();
        var products = await query.OrderBy(p => p.Name).ThenBy(p => p.Id)
            .Skip((page - 1) * 12).Take(12)
            .Select(p => new CatalogProductCard(p.Id, p.Name, p.Price, p.Stock, p.Category.Name,
                p.ShopProductImages.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()))
            .ToListAsync();
        var categories = await db.ShopCategories.AsNoTracking().Where(c => c.IsActive)
            .OrderBy(c => c.Name).Select(c => new CatalogCategoryOption(c.Id, c.Name)).ToListAsync();
        return View(new CatalogIndexViewModel(products, categories, search, categoryId, page, total));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await db.ShopCatalogProducts.AsNoTracking()
            .Where(p => p.Id == id && p.IsPublished && p.Category.IsActive)
            .Select(p => new CatalogProductDetails(
                p.Id, p.Sku, p.Name, p.Description, p.Price, p.Stock,
                p.Category.Name, p.Brand != null ? p.Brand.Name : null,
                p.ShopProductImages.OrderBy(i => i.SortOrder)
                    .Select(i => new CatalogProductImage(i.ImageUrl, i.AltText)).ToList()))
            .FirstOrDefaultAsync();
        return product == null ? NotFound() : View(product);
    }
}

public record CatalogProductImage(string Url, string? Alt);
public record CatalogProductDetails(
    int Id, string Sku, string Name, string? Description, decimal Price, decimal Stock,
    string Category, string? Brand, IReadOnlyList<CatalogProductImage> Images);

public record CatalogProductCard(int Id, string Name, decimal Price, decimal Stock, string Category, string? ImageUrl);
public record CatalogCategoryOption(int Id, string Name);
public record CatalogIndexViewModel(
    IReadOnlyList<CatalogProductCard> Products,
    IReadOnlyList<CatalogCategoryOption> Categories,
    string? Search, int? CategoryId, int Page, int Total)
{
    public int TotalPages => (Total + 11) / 12;
}
