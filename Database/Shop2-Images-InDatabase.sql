/* Run once against backed-up Shop development database.
   Additive migration: existing ImageUrl rows remain readable. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.ShopProductImages', N'U') IS NULL
    THROW 50001, 'Run Shop2-Catalog.sql first.', 1;
IF COL_LENGTH('dbo.ShopProductImages', 'ImageData') IS NULL
    ALTER TABLE dbo.ShopProductImages ADD ImageData varbinary(max) NULL;
IF COL_LENGTH('dbo.ShopProductImages', 'ContentType') IS NULL
    ALTER TABLE dbo.ShopProductImages ADD ContentType nvarchar(50) NULL;
IF COL_LENGTH('dbo.ShopProductImages', 'FileName') IS NULL
    ALTER TABLE dbo.ShopProductImages ADD FileName nvarchar(255) NULL;
COMMIT TRANSACTION;
