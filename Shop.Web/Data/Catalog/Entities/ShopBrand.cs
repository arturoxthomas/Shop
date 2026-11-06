using System;
using System.Collections.Generic;

namespace Shop.Web.Data.Catalog.Entities;

public partial class ShopBrand
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ShopCatalogProduct> ShopCatalogProducts { get; set; } = new List<ShopCatalogProduct>();
}
