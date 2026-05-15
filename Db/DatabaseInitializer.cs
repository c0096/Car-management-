using Microsoft.Data.SqlClient;

namespace Orders.Db;

public sealed class DatabaseInitializer(ISqlConnectionFactory connectionFactory) : IDatabaseInitializer
{
    public async Task InitializeAsync()
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= 30; attempt++)
        {
            try
            {
                await using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync();
                await ExecuteSchemaAsync(connection);
                return;
            }
            catch (SqlException exception)
            {
                lastException = exception;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        throw new InvalidOperationException("Unable to initialize the SQL Server schema.", lastException);
    }

    private static async Task ExecuteSchemaAsync(SqlConnection connection)
    {
        var commandText = """
            IF OBJECT_ID('dbo.Users', 'U') IS NULL AND OBJECT_ID('dbo.AppUsers', 'U') IS NOT NULL
            BEGIN
                EXEC sp_rename 'dbo.AppUsers', 'Users';
            END;

            IF OBJECT_ID('dbo.Orders', 'U') IS NULL AND OBJECT_ID('dbo.VehicleSaleDeclarations', 'U') IS NOT NULL
            BEGIN
                EXEC sp_rename 'dbo.VehicleSaleDeclarations', 'Orders';
            END;

            IF OBJECT_ID('dbo.OrderAttachments', 'U') IS NULL AND OBJECT_ID('dbo.DeclarationAttachments', 'U') IS NOT NULL
            BEGIN
                EXEC sp_rename 'dbo.DeclarationAttachments', 'OrderAttachments';
            END;

            IF COL_LENGTH('dbo.Orders', 'DeclarationDateTime') IS NOT NULL AND COL_LENGTH('dbo.Orders', 'OrderDateTime') IS NULL
            BEGIN
                EXEC sp_rename 'dbo.Orders.DeclarationDateTime', 'OrderDateTime', 'COLUMN';
            END;

            IF COL_LENGTH('dbo.OrderAttachments', 'DeclarationId') IS NOT NULL AND COL_LENGTH('dbo.OrderAttachments', 'OrderId') IS NULL
            BEGIN
                EXEC sp_rename 'dbo.OrderAttachments.DeclarationId', 'OrderId', 'COLUMN';
            END;

            IF OBJECT_ID('dbo.Users', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users
                (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Email NVARCHAR(256) NOT NULL,
                    NormalizedEmail NVARCHAR(256) NOT NULL,
                    PasswordHash NVARCHAR(500) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
                );
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_Users_NormalizedEmail'
                    AND object_id = OBJECT_ID('dbo.Users')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_Users_NormalizedEmail
                ON dbo.Users (NormalizedEmail);
            END;

            IF OBJECT_ID('dbo.OrderAttachments', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.OrderAttachments
                (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    OrderId INT NOT NULL,
                    OriginalFileName NVARCHAR(260) NOT NULL,
                    StoredFileName NVARCHAR(260) NOT NULL,
                    ContentType NVARCHAR(120) NOT NULL,
                    SizeBytes BIGINT NOT NULL,
                    RelativePath NVARCHAR(500) NOT NULL,
                    UploadedAt DATETIME2 NOT NULL CONSTRAINT DF_OrderAttachments_UploadedAt DEFAULT SYSUTCDATETIME()
                );
            END;

            IF OBJECT_ID('dbo.Orders', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Orders
                (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    WriterName NVARCHAR(150) NOT NULL,
                    AuthorizationNumber NVARCHAR(80) NOT NULL,
                    WriterPhone NVARCHAR(40) NOT NULL,
                    City NVARCHAR(120) NOT NULL,
                    OrderDateTime DATETIME2 NOT NULL,
                    SellerName NVARCHAR(150) NOT NULL,
                    SellerAddress NVARCHAR(250) NOT NULL,
                    SellerCin NVARCHAR(80) NOT NULL,
                    SellerPhone NVARCHAR(40) NOT NULL,
                    SoldItemDescription NVARCHAR(250) NOT NULL,
                    OrderNumber NVARCHAR(80) NOT NULL,
                    VehicleType NVARCHAR(120) NOT NULL,
                    VehicleBrand NVARCHAR(120) NOT NULL,
                    ChassisNumber NVARCHAR(120) NOT NULL,
                    BuyerName NVARCHAR(150) NOT NULL,
                    BuyerAddress NVARCHAR(250) NOT NULL,
                    BuyerCin NVARCHAR(80) NOT NULL,
                    BuyerPhone NVARCHAR(40) NOT NULL,
                    PropertyTitle NVARCHAR(180) NOT NULL,
                    Observation NVARCHAR(1000) NULL,
                    SellerSignature NVARCHAR(150) NOT NULL,
                    ManagerSignature NVARCHAR(150) NOT NULL,
                    BuyerSignature NVARCHAR(150) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),
                    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Orders_UpdatedAt DEFAULT SYSUTCDATETIME()
                );
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.foreign_keys
                WHERE name = 'FK_OrderAttachments_Orders'
            )
            BEGIN
                ALTER TABLE dbo.OrderAttachments
                ADD CONSTRAINT FK_OrderAttachments_Orders
                FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id)
                ON DELETE CASCADE;
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'IX_Orders_OrderDateTime'
                    AND object_id = OBJECT_ID('dbo.Orders')
            )
            BEGIN
                CREATE INDEX IX_Orders_OrderDateTime
                ON dbo.Orders (OrderDateTime DESC);
            END;

            IF OBJECT_ID('dbo.Categories', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Categories
                (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Name NVARCHAR(120) NOT NULL,
                    Description NVARCHAR(500) NULL,
                    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT SYSUTCDATETIME(),
                    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Categories_UpdatedAt DEFAULT SYSUTCDATETIME()
                );
            END;

            IF OBJECT_ID('dbo.Products', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Products
                (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Name NVARCHAR(160) NOT NULL,
                    Sku NVARCHAR(80) NOT NULL,
                    Description NVARCHAR(700) NULL,
                    CategoryId INT NOT NULL,
                    UnitPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Products_UnitPrice DEFAULT 0,
                    StockQuantity INT NOT NULL CONSTRAINT DF_Products_StockQuantity DEFAULT 0,
                    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
                    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT SYSUTCDATETIME()
                );
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.foreign_keys
                WHERE name = 'FK_Products_Categories'
            )
            BEGIN
                ALTER TABLE dbo.Products
                ADD CONSTRAINT FK_Products_Categories
                FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id);
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_Products_Sku'
                    AND object_id = OBJECT_ID('dbo.Products')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_Products_Sku
                ON dbo.Products (Sku);
            END;

            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE name = 'UX_Categories_Name'
                    AND object_id = OBJECT_ID('dbo.Categories')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_Categories_Name
                ON dbo.Categories (Name);
            END;
            """;

        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }
}
