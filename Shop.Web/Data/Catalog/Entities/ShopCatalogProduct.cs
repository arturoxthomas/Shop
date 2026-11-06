using System;
using System.Collections.Generic;

namespace Shop.Web.Data.Catalog.Entities;

public partial class ShopCatalogProduct
{
    public int Id { get; set; }

    public string Sku { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }

    public string Description { get; set; }

    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public decimal Price { get; set; }

    public decimal Stock { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ShopBrand Brand { get; set; }

    public virtual ShopCategory Category { get; set; }

    public virtual ICollection<ShopProductImage> ShopProductImages { get; set; } = new List<ShopProductImage>();
}
