IF DB_ID(N'VehicleDeclarationsDb') IS NULL
BEGIN
    CREATE DATABASE VehicleDeclarationsDb;
END
GO

USE VehicleDeclarationsDb;
GO

IF OBJECT_ID('dbo.AppUsers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Email NVARCHAR(256) NOT NULL,
        NormalizedEmail NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AppUsers_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_AppUsers_NormalizedEmail'
        AND object_id = OBJECT_ID('dbo.AppUsers')
)
BEGIN
    CREATE UNIQUE INDEX UX_AppUsers_NormalizedEmail
    ON dbo.AppUsers (NormalizedEmail);
END
GO

IF OBJECT_ID('dbo.VehicleSaleDeclarations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleSaleDeclarations
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        WriterName NVARCHAR(150) NOT NULL,
        AuthorizationNumber NVARCHAR(80) NOT NULL,
        WriterPhone NVARCHAR(40) NOT NULL,
        City NVARCHAR(120) NOT NULL,
        DeclarationDateTime DATETIME2 NOT NULL,
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
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_VehicleSaleDeclarations_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_VehicleSaleDeclarations_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID('dbo.DeclarationAttachments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeclarationAttachments
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DeclarationId INT NOT NULL,
        OriginalFileName NVARCHAR(260) NOT NULL,
        StoredFileName NVARCHAR(260) NOT NULL,
        ContentType NVARCHAR(120) NOT NULL,
        SizeBytes BIGINT NOT NULL,
        RelativePath NVARCHAR(500) NOT NULL,
        UploadedAt DATETIME2 NOT NULL CONSTRAINT DF_DeclarationAttachments_UploadedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DeclarationAttachments_VehicleSaleDeclarations FOREIGN KEY (DeclarationId)
            REFERENCES dbo.VehicleSaleDeclarations(Id)
            ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_VehicleSaleDeclarations_DeclarationDateTime'
        AND object_id = OBJECT_ID('dbo.VehicleSaleDeclarations')
)
BEGIN
    CREATE INDEX IX_VehicleSaleDeclarations_DeclarationDateTime
    ON dbo.VehicleSaleDeclarations (DeclarationDateTime DESC);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_VehicleSaleDeclarations_OrderNumber'
        AND object_id = OBJECT_ID('dbo.VehicleSaleDeclarations')
)
BEGIN
    CREATE INDEX IX_VehicleSaleDeclarations_OrderNumber
    ON dbo.VehicleSaleDeclarations (OrderNumber);
END
GO
