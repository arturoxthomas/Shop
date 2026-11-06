using Microsoft.AspNetCore.Authorization;
using Shop.Web.Controllers;
using Xunit;

namespace Shop.Web.Tests;

public class ProductAuthorizationTests
{
    [Fact]
    public void MvcProductsController_RequiresAuthentication()
    {
        var attribute = Attribute.GetCustomAttribute(
            typeof(ProductsController),
            typeof(AuthorizeAttribute),
            inherit: true);

        Assert.NotNull(attribute);
    }

    [Fact]
    public void ApiProductsController_RequiresAuthentication()
    {
        var attribute = Attribute.GetCustomAttribute(
            typeof(Shop.Web.Controllers.API.ProductsController),
            typeof(AuthorizeAttribute),
            inherit: true);

        Assert.NotNull(attribute);
    }
}
