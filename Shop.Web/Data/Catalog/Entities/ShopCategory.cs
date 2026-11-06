using System;
using System.Collections.Generic;

namespace Shop.Web.Data.Catalog.Entities;

public partial class ShopCategory
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Slug { get; set; }

    public string Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ShopCategory> InverseParentCategory { get; set; } = new List<ShopCategory>();

    public virtual ShopCategory ParentCategory { get; set; }

    public virtual ICollection<ShopCatalogProduct> ShopCatalogProducts { get; set; } = new List<ShopCatalogProduct>();
}
