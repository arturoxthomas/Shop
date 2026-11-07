/* Shop 2.0 - orders. Execute on a backed-up development Shop database. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.ShopOrders',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopOrders (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopOrders PRIMARY KEY,
  CustomerName nvarchar(150) NOT NULL,
  CustomerEmail nvarchar(254) NOT NULL,
  Phone nvarchar(40) NULL,
  ShippingAddress nvarchar(500) NOT NULL,
  Notes nvarchar(1000) NULL,
  Status nvarchar(20) NOT NULL CONSTRAINT DF_ShopOrders_Status DEFAULT ('Pending'),
  Total decimal(18,2) NOT NULL,
  CreatedAtUtc datetime2(0) NOT NULL CONSTRAINT DF_ShopOrders_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
  CONSTRAINT CK_ShopOrders_Status CHECK (Status IN ('Pending','Confirmed','Shipped','Delivered','Cancelled')),
  CONSTRAINT CK_ShopOrders_Total CHECK (Total >= 0)
 );
END;
IF OBJECT_ID(N'dbo.ShopOrderItems',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.ShopOrderItems (
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShopOrderItems PRIMARY KEY,
  OrderId int NOT NULL,
  ProductId int NOT NULL,
  ProductName nvarchar(150) NOT NULL,
  Sku nvarchar(50) NOT NULL,
  Quantity int NOT NULL,
  UnitPrice decimal(18,2) NOT NULL,
  Subtotal AS (CONVERT(decimal(18,2), Quantity * UnitPrice)) PERSISTED,
  CONSTRAINT FK_ShopOrderItems_Order FOREIGN KEY (OrderId) REFERENCES dbo.ShopOrders(Id),
  CONSTRAINT FK_ShopOrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.ShopCatalogProducts(Id),
  CONSTRAINT CK_ShopOrderItems_Quantity CHECK (Quantity > 0),
  CONSTRAINT CK_ShopOrderItems_UnitPrice CHECK (UnitPrice >= 0)
 );
 CREATE INDEX IX_ShopOrderItems_OrderId ON dbo.ShopOrderItems(OrderId);
END;
COMMIT TRANSACTION;
