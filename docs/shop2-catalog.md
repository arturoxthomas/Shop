# Shop 2.0 — catálogo Database-First

## Alcance inicial

La tienda general tendrá categorías jerárquicas, marcas, productos con SKU único, precios decimales, stock, publicación e imágenes múltiples. El modelo nuevo usa tablas `Shop*` para evitar conflictos con el esquema histórico y con ASP.NET Core Identity.

## Instalación controlada

1. Realizar respaldo de la base `Shop` local.
2. Revisar `Database/Shop2-Catalog.sql` y ejecutarlo en SSMS sobre `Shop` (no sobre `master`). El script no incluye `CREATE DATABASE` y no altera `Products` ni las tablas Identity.
3. Confirmar que existen `ShopCategories`, `ShopBrands`, `ShopCatalogProducts`, `ShopProductImages`.
4. Desde la raíz del repositorio, en PowerShell, definir temporalmente la conexión a la instancia local:

```powershell
$env:SHOP_DB_CONNECTION = 'Server=localhost;Database=Shop;Trusted_Connection=True;TrustServerCertificate=True'
.\scripts\Scaffold-ShopCatalog.ps1
Remove-Item Env:SHOP_DB_CONNECTION
```

Sustituir `localhost` por el nombre de la instancia correspondiente. No subir credenciales ni archivos de configuración local.

## Próximos cambios

- Revisar entidades generadas y registrar `ShopCatalogContext` para lectura de catálogo sin interferir con `DataContext` de Identity.
- Agregar administración autenticada de categorías, marcas y productos, con validaciones y pruebas.
- Agregar escaparate público, filtros, paginación e imágenes con validación de URL.
- Después desarrollar carrito, clientes y pedidos con inventario transaccional.

## Límites actuales

El esquema y el script de scaffolding están versionados, pero **aún no se ejecutaron** en SQL Server ni se validaron con integración real. Las vistas y los controladores del catálogo nuevo aún no existen. La tabla histórica `Products` permanece independiente; cualquier migración de datos debe diseñarse y probarse antes de ejecutarse.
