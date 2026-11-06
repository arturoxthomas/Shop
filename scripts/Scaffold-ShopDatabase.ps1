param(
    [Parameter(Mandatory = $false)]
    [string]$ConnectionString = $env:SHOP_DB_CONNECTION
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "Set SHOP_DB_CONNECTION or pass -ConnectionString. Never commit database credentials."
}

Push-Location (Join-Path $PSScriptRoot "..")
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }

    dotnet ef dbcontext scaffold $ConnectionString Microsoft.EntityFrameworkCore.SqlServer --project Shop.Web/Shop.Web.csproj --startup-project Shop.Web/Shop.Web.csproj --output-dir Data/DatabaseFirst/Entities --context-dir Data/DatabaseFirst --context ShopDatabaseContext --namespace Shop.Web.Data.DatabaseFirst.Entities --context-namespace Shop.Web.Data.DatabaseFirst --table Products --table Countries --no-onconfiguring --use-database-names --force
    if ($LASTEXITCODE -ne 0) { throw "Database scaffolding failed." }

    Write-Host "Database-first reference model generated in Shop.Web/Data/DatabaseFirst."
    Write-Host "This model is intentionally separate from the active Identity/DataContext until reviewed and integrated."
}
finally {
    Pop-Location
}
