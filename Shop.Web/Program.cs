using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Web.Data;
using Shop.Web.Data.Entities;
using Shop.Web.Data.Catalog;
using Shop.Web.Helpers;
using Shop.Web.Services;
using Shop.Web.Data.Orders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The 'DefaultConnection' connection string is missing. Configure ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

// Database-First catalog context; Identity and legacy products stay on DataContext.
builder.Services.AddDbContext<ShopCatalogContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ShopOrdersContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequiredUniqueChars = 4;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddDefaultUI()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-Shop.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ShopCartService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Explicit, development-only administrator setup. Never grant roles on normal startup.
if (args.Length == 2 && args[0] == "--grant-shop-admin")
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Administrator setup is only available in Development.");

    var email = args[1].Trim();
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Provide the existing user's email.");

    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var user = await users.FindByEmailAsync(email);
    if (user == null || !user.EmailConfirmed)
        throw new InvalidOperationException("User not found or email not confirmed. No permissions changed.");

    if (!await roles.RoleExistsAsync("Admin"))
    {
        var created = await roles.CreateAsync(new IdentityRole("Admin"));
        if (!created.Succeeded)
            throw new InvalidOperationException("Could not create Admin role: " +
                string.Join("; ", created.Errors.Select(e => e.Description)));
    }

    if (!await users.IsInRoleAsync(user, "Admin"))
    {
        var assigned = await users.AddToRoleAsync(user, "Admin");
        if (!assigned.Succeeded)
            throw new InvalidOperationException("Could not grant Admin role: " +
                string.Join("; ", assigned.Errors.Select(e => e.Description)));
    }

    Console.WriteLine("Admin role assigned to existing confirmed user. Sign out and sign in again.");
    return;
}

app.Run();
