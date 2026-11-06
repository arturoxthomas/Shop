/*
    SHOP - SQL Server schema and migration preflight
    READ-ONLY: this script does not create, alter, update, or delete data.
    Run it against a RESTORED COPY or an approved database connection first.
*/
SET NOCOUNT ON;

PRINT '1. Migration history';
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
BEGIN
    SELECT [MigrationId], [ProductVersion]
    FROM dbo.__EFMigrationsHistory
    ORDER BY [MigrationId];
END
ELSE
BEGIN
    PRINT 'No dbo.__EFMigrationsHistory table found.';
END;

PRINT '2. User tables';
SELECT s.name AS [SchemaName], t.name AS [TableName]
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

PRINT '3. Columns for Shop tables';
SELECT
    s.name AS [SchemaName],
    t.name AS [TableName],
    c.column_id AS [Ordinal],
    c.name AS [ColumnName],
    TYPE_NAME(c.user_type_id) AS [SqlType],
    c.max_length AS [MaxLengthBytes],
    c.precision AS [Precision],
    c.scale AS [Scale],
    c.is_nullable AS [IsNullable],
    c.is_identity AS [IsIdentity]
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.columns AS c ON c.object_id = t.object_id
WHERE t.name IN
(
    N'Products', N'Countries', N'AspNetUsers', N'AspNetRoles',
    N'AspNetUserClaims', N'AspNetUserLogins', N'AspNetUserRoles',
    N'AspNetUserTokens', N'AspNetRoleClaims'
)
ORDER BY s.name, t.name, c.column_id;

PRINT '4. Foreign keys';
SELECT
    fk.name AS [ForeignKey],
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS [ParentSchema],
    OBJECT_NAME(fk.parent_object_id) AS [ParentTable],
    pc.name AS [ParentColumn],
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS [ReferencedSchema],
    OBJECT_NAME(fk.referenced_object_id) AS [ReferencedTable],
    rc.name AS [ReferencedColumn],
    fk.delete_referential_action_desc AS [OnDelete]
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns AS pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
JOIN sys.columns AS rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
WHERE OBJECT_NAME(fk.parent_object_id) IN (N'Products', N'AspNetUserClaims', N'AspNetUserLogins', N'AspNetUserRoles', N'AspNetUserTokens', N'AspNetRoleClaims')
ORDER BY [ParentTable], [ForeignKey];

PRINT '5. Row counts for Shop tables';
SELECT
    s.name AS [SchemaName],
    t.name AS [TableName],
    SUM(p.rows) AS [ApproximateRows]
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.partitions AS p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
WHERE t.name IN
(
    N'Products', N'Countries', N'AspNetUsers', N'AspNetRoles',
    N'AspNetUserClaims', N'AspNetUserLogins', N'AspNetUserRoles',
    N'AspNetUserTokens', N'AspNetRoleClaims'
)
GROUP BY s.name, t.name
ORDER BY s.name, t.name;
