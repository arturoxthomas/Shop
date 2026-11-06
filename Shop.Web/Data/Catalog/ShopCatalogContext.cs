using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data.Catalog.Entities;

namespace Shop.Web.Data.Catalog;

public partial class ShopCatalogContext : DbContext
{
    public ShopCatalogContext(DbContextOptions<ShopCatalogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ShopBrand> ShopBrands { get; set; }

    public virtual DbSet<ShopCatalogProduct> ShopCatalogProducts { get; set; }

    public virtual DbSet<ShopCategory> ShopCategories { get; set; }

    public virtual DbSet<ShopProductImage> ShopProductImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShopBrand>(entity =>
        {
            entity.HasIndex(e => e.Slug, "UQ_ShopBrands_Slug").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ShopBrands_IsActive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120);
        });

        modelBuilder.Entity<ShopCatalogProduct>(entity =>
        {
            entity.HasIndex(e => e.BrandId, "IX_ShopCatalogProducts_BrandId");

            entity.HasIndex(e => e.CategoryId, "IX_ShopCatalogProducts_CategoryId");

            entity.HasIndex(e => e.Sku, "UQ_ShopCatalogProducts_Sku").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ_ShopCatalogProducts_Slug").IsUnique();

            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ShopCatalogProducts_CreatedAtUtc");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(180);
            entity.Property(e => e.Stock).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Brand).WithMany(p => p.ShopCatalogProducts)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK_ShopCatalogProducts_Brand");

            entity.HasOne(d => d.Category).WithMany(p => p.ShopCatalogProducts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShopCatalogProducts_Category");
        });

        modelBuilder.Entity<ShopCategory>(entity =>
        {
            entity.HasIndex(e => e.Slug, "UQ_ShopCategories_Slug").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_ShopCategories_IsActive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_ShopCategories_Parent");
        });

        modelBuilder.Entity<ShopProductImage>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_ShopProductImages_ProductId");

            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(2048);

            entity.HasOne(d => d.Product).WithMany(p => p.ShopProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ShopProductImages_Product");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
