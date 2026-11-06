param([string]$ConnectionString = $env:SHOP_DB_CONNECTION)
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "Set SHOP_DB_CONNECTION or pass -ConnectionString. Do not commit credentials."
}
Push-Location (Join-Path $PSScriptRoot "..")
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }
    dotnet ef dbcontext scaffold $ConnectionString Microsoft.EntityFrameworkCore.SqlServer --project Shop.Web/Shop.Web.csproj --startup-project Shop.Web/Shop.Web.csproj --output-dir Data/Catalog/Entities --context-dir Data/Catalog --context ShopCatalogContext --namespace Shop.Web.Data.Catalog.Entities --context-namespace Shop.Web.Data.Catalog --table ShopCategories --table ShopBrands --table ShopCatalogProducts --table ShopProductImages --no-onconfiguring --force
    if ($LASTEXITCODE -ne 0) { throw "Catalog scaffolding failed." }
    Write-Host "Catalog model generated. Review generated classes before integration."
}
finally { Pop-Location }
