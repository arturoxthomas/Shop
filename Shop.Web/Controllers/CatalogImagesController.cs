using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog;

namespace Shop.Web.Controllers;

[Route("catalog/images")]
public sealed class CatalogImagesController : Controller
{
    private readonly ShopCatalogContext db;
    public CatalogImagesController(ShopCatalogContext db) => this.db = db;

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Get(int id)
    {
        var image = await db.ShopProductImages.AsNoTracking()
            .Where(i => i.Id == id && i.Product.IsPublished && i.Product.Category.IsActive)
            .Select(i => new { i.ImageData, i.ContentType }).FirstOrDefaultAsync();
        if (image?.ImageData == null || image.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            return NotFound();
        return File(image.ImageData, image.ContentType);
    }
}
