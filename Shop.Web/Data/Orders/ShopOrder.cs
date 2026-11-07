namespace Shop.Web.Data.Orders;

public sealed class ShopOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string? Phone { get; set; }
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<ShopOrderItem> Items { get; set; } = new();
}
public sealed class ShopOrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
