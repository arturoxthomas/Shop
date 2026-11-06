using Microsoft.EntityFrameworkCore;
using Shop.Web.Data;
using Shop.Web.Data.Entities;
using Xunit;

namespace Shop.Web.Tests;

public class ProductRepositoryTests
{
    [Fact]
    public async Task GetAllByUserEmail_ReturnsOnlyProductsOwnedByRequestedUser()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new DataContext(options);
        var alice = new User
        {
            Id = "alice-id",
            UserName = "alice@example.com",
            Email = "alice@example.com"
        };
        var bob = new User
        {
            Id = "bob-id",
            UserName = "bob@example.com",
            Email = "bob@example.com"
        };

        context.Users.AddRange(alice, bob);
        context.Products.AddRange(
            new Product { Name = "Alice product", User = alice },
            new Product { Name = "Bob product", User = bob });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var products = await repository.GetAllByUserEmail("alice@example.com").ToListAsync();

        var product = Assert.Single(products);
        Assert.Equal("Alice product", product.Name);
    }

    [Fact]
    public async Task GetByIdAndUserEmailAsync_DoesNotReturnAnotherUsersProduct()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new DataContext(options);
        var alice = new User
        {
            Id = "alice-id",
            UserName = "alice@example.com",
            Email = "alice@example.com"
        };
        var bob = new User
        {
            Id = "bob-id",
            UserName = "bob@example.com",
            Email = "bob@example.com"
        };

        context.Users.AddRange(alice, bob);
        var alicesProduct = new Product { Name = "Alice product", User = alice };
        var bobsProduct = new Product { Name = "Bob product", User = bob };
        context.Products.AddRange(alicesProduct, bobsProduct);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var ownProduct = await repository.GetByIdAndUserEmailAsync(
            alicesProduct.Id,
            "alice@example.com");
        var foreignProduct = await repository.GetByIdAndUserEmailAsync(
            bobsProduct.Id,
            "alice@example.com");

        Assert.NotNull(ownProduct);
        Assert.Equal("Alice product", ownProduct.Name);
        Assert.Null(foreignProduct);
    }
}
