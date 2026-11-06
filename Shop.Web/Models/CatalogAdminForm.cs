using System.ComponentModel.DataAnnotations;

namespace Shop.Web.Models;

public sealed class CatalogAdminForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "El identificador URL es obligatorio.")]
    [StringLength(120)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Usa letras minúsculas, números y guiones.")]
    public string Slug { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
