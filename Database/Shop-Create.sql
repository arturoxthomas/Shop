IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [LastPurchase] datetime2 NOT NULL,
    [LastSale] datetime2 NOT NULL,
    [IsAvailabe] bit NOT NULL,
    [Stock] float NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190614162222_InitialDb', N'10.0.14');
COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(50) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'LastSale');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Products] ALTER COLUMN [LastSale] datetime2 NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'LastPurchase');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Products] ALTER COLUMN [LastPurchase] datetime2 NULL;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190614221312_ModifyProducts', N'10.0.14');
COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Products] ADD [UserId] nvarchar(450) NULL;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL, [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL, [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL, [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL, [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL, [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL, [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL, [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL, [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL, [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL, [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY, [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY, [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL, [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL, [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL, [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL, [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL, [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL, [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_Products_UserId] ON [Products] ([UserId]);
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190629175102_Users', N'10.0.14');
COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Countries] (
    [Id] int NOT NULL IDENTITY, [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
);
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190701182619_AddCountriesModel', N'10.0.14');
COMMIT;
GO
