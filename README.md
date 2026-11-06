# Shop

Shop is a learning and portfolio e-commerce application. It started as an ASP.NET Core 2.1 MVC project with Entity Framework Core, SQL Server and ASP.NET Identity. The repository also preserves legacy Xamarin.Forms clients.

## Modernization status

The modernization work is taking place on `modernizacion/net10`; the historical `master` branch remains unchanged.

- Web target: **.NET 10**
- Persistence: **Entity Framework Core 10 + SQL Server**
- Authentication: **ASP.NET Core Identity**
- UI: **ASP.NET Core MVC + Identity UI**
- Architecture: MVC controllers, API endpoints and repository layer

> This branch is under active development. It has not yet been certified by a successful build or end-to-end test. Do not deploy it to production.

## Features being recovered

- Product catalog with create, read, update and delete screens
- Product stock, availability and pricing fields
- Individual user accounts
- User-scoped product management
- Read-only products API
- SQL Server persistence and EF Core migrations
- Historical Xamarin.Forms Android/iOS clients (not part of the current .NET 10 web build)

## Requirements

- .NET 10 SDK
- SQL Server LocalDB on Windows for the default development connection, or another SQL Server instance
- Git

## Run locally

1. Clone the repository and switch to `modernizacion/net10`.
2. Restore dependencies:

   ```bash
   dotnet restore Shop.Web/Shop.Web.csproj
   ```

3. On Windows, the development configuration uses SQL Server LocalDB. For another SQL Server instance, set the environment variable `ConnectionStrings__DefaultConnection` before starting the application.

   PowerShell example:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=Shop;Trusted_Connection=True;TrustServerCertificate=True"
   ```

4. Apply the database migrations after confirming that the migration history matches the target database:

   ```bash
   dotnet tool restore
   dotnet ef database update --project Shop.Web/Shop.Web.csproj
   ```

5. Start the web application:

   ```bash
   dotnet run --project Shop.Web/Shop.Web.csproj
   ```

6. Open the local HTTPS URL printed by the application. Register an account through `/Identity/Account/Register`, then manage products through `/Products`.

Run the automated tests with:

   ```bash
   dotnet test Shop.Web.Tests/Shop.Web.Tests.csproj --configuration Release
   ```

The committed LocalDB connection is for local development only. Production connection strings must be injected through environment variables or a managed secret store; never commit production credentials.

## Security work completed so far

- Removed the hardcoded demo account initializer.
- Removed hardcoded email identities from product controllers.
- Required authentication for product MVC and API endpoints.
- Added owner-scoped product reads, updates and deletes.
- Strengthened password and lockout defaults.
- Configured secure, HTTP-only authentication cookies.
- Added the ASP.NET Core Identity login and registration UI.

These changes are an initial baseline, not a completed security audit. The historical public Git history may still contain personal information and a demo credential; deleting the current file does not erase earlier commits. A separate history review and cleanup is still required.

## Automated tests

The initial test suite verifies that product queries return only records owned by the authenticated account and that both MVC and API product controllers require authentication. The tests use EF Core InMemory; database integration and browser-based end-to-end tests are still pending.

## Next steps

1. Validate the EF Core 2.1 migrations against EF Core 10 and generate a clean migration baseline if required.
2. Run the .NET 10 build and fix all compile/runtime issues.
3. Add automated tests for authentication, authorization and ownership boundaries.
4. Improve the UI, accessibility and responsive layout.
5. Add product image handling, search, filtering and pagination.
6. Publish a hosted demo only after the security and deployment reviews.

## Security

Read [SECURITY.md](SECURITY.md) before reporting a vulnerability.
