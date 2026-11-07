using System.ComponentModel.DataAnnotations;

namespace Shop.Web.Models;

public sealed class CheckoutForm
{
    [Required(ErrorMessage = "Ingresa tu nombre.")]
    [StringLength(150)]
    public string CustomerName { get; set; } = "";
    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [StringLength(254)]
    public string CustomerEmail { get; set; } = "";
    [StringLength(40)]
    public string? Phone { get; set; }
    [Required(ErrorMessage = "Ingresa la dirección de entrega.")]
    [StringLength(500)]
    public string ShippingAddress { get; set; } = "";
    [StringLength(1000)]
    public string? Notes { get; set; }
}
