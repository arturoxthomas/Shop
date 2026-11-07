using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Shop.Web.Models;

public sealed class CatalogProductForm
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string Sku { get; set; } = "";
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, StringLength(180), RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")] public string Slug { get; set; } = "";
    public string? Description { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")] public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "El precio debe ser positivo o cero.")] public decimal Price { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "El inventario debe ser positivo o cero.")] public decimal Stock { get; set; }
    public bool IsPublished { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    public List<CatalogImageItem> ExistingImages { get; set; } = new();
    public List<int> RemoveImageIds { get; set; } = new();
}

public sealed record CatalogImageItem(int Id, string Url);
