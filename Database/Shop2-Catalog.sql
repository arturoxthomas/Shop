/*
 Shop 2.0 - Catalog foundation (SQL Server / Database-First)
 Apply ONLY to a backed-up Shop development database after review.
 This script is repeatable for catalog tables, and does not change legacy Products or Identity.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.ShopCategories', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopCategories (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopCategories PRIMARY KEY,
  Name nvarchar(100) NOT NULL,
  Slug nvarchar(120) NOT NULL,
  Description nvarchar(500) NULL,
  ParentCategoryId int NULL,
  IsActive bit NOT NULL CONSTRAINT DF_ShopCategories_IsActive DEFAULT (1),
  CONSTRAINT FK_ShopCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES dbo.ShopCategories(Id),
  CONSTRAINT UQ_ShopCategories_Slug UNIQUE (Slug)
 );
END;

IF OBJECT_ID(N'dbo.ShopBrands', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopBrands (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopBrands PRIMARY KEY,
  Name nvarchar(100) NOT NULL,
  Slug nvarchar(120) NOT NULL,
  IsActive bit NOT NULL CONSTRAINT DF_ShopBrands_IsActive DEFAULT (1),
  CONSTRAINT UQ_ShopBrands_Slug UNIQUE (Slug)
 );
END;

IF OBJECT_ID(N'dbo.ShopCatalogProducts', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopCatalogProducts (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopCatalogProducts PRIMARY KEY,
  Sku nvarchar(50) NOT NULL,
  Name nvarchar(150) NOT NULL,
  Slug nvarchar(180) NOT NULL,
  Description nvarchar(max) NULL,
  CategoryId int NOT NULL,
  BrandId int NULL,
  Price decimal(18,2) NOT NULL,
  Stock decimal(18,2) NOT NULL CONSTRAINT DF_ShopCatalogProducts_Stock DEFAULT (0),
  IsPublished bit NOT NULL CONSTRAINT DF_ShopCatalogProducts_IsPublished DEFAULT (0),
  CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_ShopCatalogProducts_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
  CONSTRAINT FK_ShopCatalogProducts_Category FOREIGN KEY (CategoryId) REFERENCES dbo.ShopCategories(Id),
  CONSTRAINT FK_ShopCatalogProducts_Brand FOREIGN KEY (BrandId) REFERENCES dbo.ShopBrands(Id),
  CONSTRAINT CK_ShopCatalogProducts_Price CHECK (Price >= 0),
  CONSTRAINT CK_ShopCatalogProducts_Stock CHECK (Stock >= 0),
  CONSTRAINT UQ_ShopCatalogProducts_Sku UNIQUE (Sku),
  CONSTRAINT UQ_ShopCatalogProducts_Slug UNIQUE (Slug)
 );
 CREATE INDEX IX_ShopCatalogProducts_CategoryId ON dbo.ShopCatalogProducts(CategoryId);
 CREATE INDEX IX_ShopCatalogProducts_BrandId ON dbo.ShopCatalogProducts(BrandId);
END;

IF OBJECT_ID(N'dbo.ShopProductImages', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopProductImages (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopProductImages PRIMARY KEY,
  ProductId int NOT NULL,
  ImageUrl nvarchar(2048) NOT NULL,
  AltText nvarchar(200) NULL,
  SortOrder int NOT NULL CONSTRAINT DF_ShopProductImages_SortOrder DEFAULT (0),
  CONSTRAINT FK_ShopProductImages_Product FOREIGN KEY (ProductId) REFERENCES dbo.ShopCatalogProducts(Id) ON DELETE CASCADE
 );
 CREATE INDEX IX_ShopProductImages_ProductId ON dbo.ShopProductImages(ProductId);
END;

COMMIT TRANSACTION;
