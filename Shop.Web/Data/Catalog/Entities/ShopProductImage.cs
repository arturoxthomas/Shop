using System;
using System.Collections.Generic;

namespace Shop.Web.Data.Catalog.Entities;

public partial class ShopProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; }

    public byte[] ImageData { get; set; }

    public string ContentType { get; set; }

    public string FileName { get; set; }

    public string AltText { get; set; }

    public int SortOrder { get; set; }

    public virtual ShopCatalogProduct Product { get; set; }
}
