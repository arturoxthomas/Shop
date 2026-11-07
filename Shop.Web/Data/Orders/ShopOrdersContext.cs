using Microsoft.EntityFrameworkCore;

namespace Shop.Web.Data.Orders;

// Database-first SQL schema: Database/Shop2-Orders.sql.
// Isolated mapping allows future re-scaffolding without changing the catalog model.
public sealed class ShopOrdersContext : DbContext
{
    public ShopOrdersContext(DbContextOptions<ShopOrdersContext> options) : base(options) { }
    public DbSet<ShopOrder> Orders => Set<ShopOrder>();
    public DbSet<ShopOrderItem> Items => Set<ShopOrderItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShopOrder>(e =>
        {
            e.ToTable("ShopOrders");
            e.HasKey(x => x.Id);
            e.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
            e.Property(x => x.CustomerEmail).HasMaxLength(254).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.ShippingAddress).HasMaxLength(500).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.Property(x => x.CreatedAtUtc).HasPrecision(0);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId);
        });
        modelBuilder.Entity<ShopOrderItem>(e =>
        {
            e.ToTable("ShopOrderItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
