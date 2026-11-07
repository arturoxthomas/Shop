using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Shop.Web.Services;

public sealed record ShopCartItem(int ProductId, int Quantity);

public sealed class ShopCartService
{
    private const string Key = "Shop.Cart.V1";
    private readonly IHttpContextAccessor accessor;
    public ShopCartService(IHttpContextAccessor accessor) => this.accessor = accessor;

    public List<ShopCartItem> Read()
    {
        var json = accessor.HttpContext?.Session.GetString(Key);
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<ShopCartItem>>(json) ?? new(); }
        catch (JsonException) { return new(); }
    }

    public void Save(IEnumerable<ShopCartItem> items)
    {
        var clean = items.Where(i => i.ProductId > 0 && i.Quantity > 0)
            .GroupBy(i => i.ProductId)
            .Select(g => new ShopCartItem(g.Key, Math.Min(99, g.Sum(x => x.Quantity))))
            .Take(50).ToList();
        accessor.HttpContext!.Session.SetString(Key, JsonSerializer.Serialize(clean));
    }
}
