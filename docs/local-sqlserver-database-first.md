# Shop: SQL Server local y transición Database-First

## Estado comprobado (noviembre de 2026)

La aplicación Shop.Web se ejecutó localmente, autenticó un usuario, consultó Products y guardó un cambio de precio de 3.00 a 3.50 en la instancia de desarrollo. Esto valida lectura y actualización con el `DataContext` actual, **no** una migración completa a Database-First.

## Configuración local sin publicar credenciales

Usar `Shop.Web/appsettings.Development.json` localmente, excluido por `.gitignore`. Ejemplo de contenido (reemplazar servidor por la instancia local):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Shop;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

El archivo ya estuvo versionado anteriormente: quitarlo del árbol actual no elimina las versiones históricas. Nunca poner contraseñas en el repositorio. Para despliegue, utilizar variables de entorno o un almacén de secretos.

## Generar entidades Database-First a partir de la base existente

El script `scripts/Scaffold-ShopDatabase.ps1` genera clases para `Products` y `Countries` en una carpeta separada del modelo activo. **No crea, borra ni modifica tablas o registros de SQL Server.** Se puede ejecutar sobre la base `Shop` existente sin volver a ejecutar `Database/Shop-Create.sql`.

En PowerShell, desde la raíz del repositorio:

```powershell
$env:SHOP_DB_CONNECTION = 'Server=localhost;Database=Shop;Trusted_Connection=True;TrustServerCertificate=True'
.\scripts\Scaffold-ShopDatabase.ps1
Remove-Item Env:SHOP_DB_CONNECTION
```

Cambiar `localhost` por el servidor e instancia propios; si el nombre contiene `\`, PowerShell lo admite dentro de comillas simples. La cadena se mantiene solo en la sesión de PowerShell y no debe añadirse al control de versiones. La ejecución requiere .NET SDK, acceso a SQL Server y herramientas EF restaurables con `dotnet tool restore`.

Los archivos esperados son:

- `Shop.Web/Data/DatabaseFirst/ShopDatabaseContext.cs`
- `Shop.Web/Data/DatabaseFirst/Entities/Product.cs`
- `Shop.Web/Data/DatabaseFirst/Entities/Country.cs`

**Importante:** este modelo es de referencia y todavía no reemplaza `Shop.Web/Data/DataContext.cs`, que también administra Identity. No registrar simultáneamente ambos contextos contra las mismas entidades sin diseñar primero la integración. Revisar tipos, claves, relaciones y nombres antes de cualquier sustitución. No ejecutar `dotnet ef database update` como parte del scaffolding.

## Comprobaciones previas a integrar

1. Revisar `git status` y excluir cambios locales no relacionados, como `Shop.sln`.
2. Confirmar que el scaffold generó las dos entidades y el contexto sin secretos embebidos.
3. Comparar las clases generadas con `Shop.Web/Data/Entities` y las consultas de `IProductRepository`.
4. Definir la separación entre contexto de negocio Database-First e Identity, con migración gradual y pruebas de aislamiento por usuario.
5. Ejecutar `dotnet test Shop.Web.Tests/Shop.Web.Tests.csproj` y `dotnet build Shop.Web/Shop.Web.csproj` antes de subir cambios.

`Database/Shop-Create.sql` se diseñó para inicializar **una base nueva y vacía**; nunca volver a ejecutarlo sobre una base ya poblada.
