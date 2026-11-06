# Shop

A small e-commerce application originally built with ASP.NET Core 2.1, Entity Framework Core, SQL Server and ASP.NET Identity. The repository also contains legacy Xamarin.Forms clients.

## Modernization status

This repository is being modernized on the `modernizacion/net10` branch. The original `master` branch is being preserved as the historical baseline.

The modernization target is **.NET 10 (LTS)**. The application is not production-ready while this work is in progress.

## Existing capabilities identified

- Product catalog with create, read, update and delete screens
- Product stock and availability fields
- ASP.NET Identity user model
- SQL Server persistence through Entity Framework Core
- Generic repository and product/country repositories
- Initial read-only products API
- Legacy Xamarin.Forms Android/iOS projects

## Planned work

1. Remove hardcoded personal data and credentials from application code and replace demo account creation with safe configuration or explicit local development setup.
2. Replace hardcoded user identities with the authenticated user's identity and enforce authorization and per-user data access.
3. Upgrade the web application to .NET 10 and supported ASP.NET Core / EF Core packages.
4. Replace legacy startup and middleware configuration with the modern hosting model.
5. Review entity validation, database migrations, API contracts, error handling and logging.
6. Add automated tests and a reproducible local development setup.
7. Refresh the UI and document how to run and demonstrate the application.
8. Review the historical public Git history for exposed personal data and credentials; removing a value from the current files does not erase it from earlier commits.

## Development

The current legacy web project targets .NET Core 2.1 and should be treated as a historical reference until the migration is completed. Do not deploy it as-is.

Connection strings and credentials must be supplied through local user secrets or environment variables. Never commit production secrets, personal contact information, database files, publish profiles or local configuration.

## Security

Please read [SECURITY.md](SECURITY.md) before using or reporting a security issue.
