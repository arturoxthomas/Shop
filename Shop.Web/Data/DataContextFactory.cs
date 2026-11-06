using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shop.Web.Data;

/// <summary>
/// Creates DataContext for EF Core design-time commands without starting the web host.
/// The connection string must come from environment variables or local configuration.
/// </summary>
public sealed class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectDirectory = File.Exists(Path.Combine(currentDirectory, "Shop.Web.csproj"))
            ? currentDirectory
            : Path.Combine(currentDirectory, "Shop.Web");

        if (!File.Exists(Path.Combine(projectDirectory, "Shop.Web.csproj")))
        {
            throw new DirectoryNotFoundException(
                "Could not locate Shop.Web.csproj. Run the EF command from the repository root or Shop.Web directory.");
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing ConnectionStrings__DefaultConnection. Configure it before running EF Core tools.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
