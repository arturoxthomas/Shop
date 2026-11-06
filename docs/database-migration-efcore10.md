# Shop database strategy: safe transition to Database-First

## Decision

Shop's original schema is represented in four EF Core 2.1 Code-First migrations. No original database backup or live database connection is currently available. Therefore the historical migration chain is the only reliable schema evidence in the repository.

The safest path is a **hybrid transition**:
- Use the historical migrations to reconstruct a fresh, disposable SQL Server database.
- Use SQL Server as the schema source for the Shop business tables (Products and Countries).
- Reverse-engineer those tables into a separate generated EF Core model using dotnet ef dbcontext scaffold.
- Keep ASP.NET Core Identity in the existing DataContext until the generated schema and Identity mappings have been checked. Identity has custom User properties (FirstName, LastName) and is not safe to replace with generic scaffolded classes blindly.

This establishes a practical Database-First workflow for business entities without pretending the entire application has already been converted.

## Step 1 - create a clean database

The GitHub Actions workflow generates Shop-Create.sql from the four existing migrations and publishes it as a workflow artifact. Download it from a successful run and execute it in SQL Server Management Studio against a **new, empty database named Shop**. The script creates the schema only; it does not recover old products, accounts, or other historical data.

Do not execute this script against a database that contains data.

## Step 2 - reverse engineer the business tables

After the empty database has been created, run the following in PowerShell from the repository root:

```powershell
$env:SHOP_DB_CONNECTION = "Server=localhost;Database=Shop;Trusted_Connection=True;TrustServerCertificate=True;"
./scripts/Scaffold-ShopDatabase.ps1
```

Use Windows authentication where possible. If SQL authentication is required, keep the connection string in a local environment variable or secret store; never commit it.

The script scaffolds only Products and Countries into:
- Shop.Web/Data/DatabaseFirst/Entities
- Shop.Web/Data/DatabaseFirst/ShopDatabaseContext.cs

It uses --no-onconfiguring, so credentials are not written into generated source files. The generated model is deliberately kept separate from the active application context until reviewed.

## Step 3 - review and integrate

Compare the generated entity properties, SQL types, nullability, keys, indexes, and relationships with the current code. Then decide whether to replace the handwritten Product/Country classes and update repository mappings, or keep separate persistence models and map them to application/domain models.

Do not run the scaffold script over handwritten source directories. The generated folder is isolated and can be regenerated with --force.

## Identity note

The current DataContext derives from IdentityDbContext<User>. The historical migration defines the Identity tables and custom FirstName / LastName columns. Keep Identity under the current EF Core Identity model for now. A full Database-First conversion of Identity would require deliberate custom mappings and integration tests; scaffolding the Identity tables as ordinary entities is not a drop-in replacement.

## What this does not do
- It does not restore any old Shop data. No backup was available.
- It does not prove the recreated schema matches a lost/unknown production database or manual SQL changes.
- It does not apply changes to any external database.
- It does not switch the running application to the generated context automatically.

## Verification checklist
- [ ] Generate Shop-Create.sql from the latest branch workflow.
- [ ] Create a brand-new empty SQL Server database from that script.
- [ ] Verify all expected tables, constraints, indexes, and migration history.
- [ ] Run Scaffold-ShopDatabase.ps1.
- [ ] Review generated entities and integrate them into repositories.
- [ ] Add SQL Server integration tests.
- [ ] Verify registration/login and product CRUD end-to-end.

The first source-control steps can be automated. Actual SQL Server database creation and reverse engineering require a SQL Server instance available to the person running the script.
