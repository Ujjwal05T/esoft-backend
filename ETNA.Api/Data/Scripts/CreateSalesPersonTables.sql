-- SQL Script to create SalesPersons, Areas and Brands tables (SIMPLIFIED - using JSON arrays)
-- Run this in SQL Server Management Studio or Azure Data Studio

-- =====================================================
-- 1. Create Areas Table
-- =====================================================
CREATE TABLE Areas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Cities NVARCHAR(MAX) NOT NULL, -- JSON array: ["Mumbai", "Thane", "Navi Mumbai"]
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IX_Areas_Name ON Areas(Name);
CREATE INDEX IX_Areas_State ON Areas(State);

PRINT 'Areas table created successfully!';

-- =====================================================
-- 2. Create Brands Table
-- =====================================================
CREATE TABLE Brands (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    LogoUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IX_Brands_Name ON Brands(Name);
CREATE INDEX IX_Brands_IsActive ON Brands(IsActive);

PRINT 'Brands table created successfully!';

-- =====================================================
-- 3. Create SalesPersons Table (with JSON array fields)
-- =====================================================
CREATE TABLE SalesPersons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Basic Info
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Sales Person', 
    -- 'Sales Person', 'Manager', 'Admin', 'Backend Support', 'Warehouse Staff'
    Department NVARCHAR(100) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Inactive'
    WorkingHours NVARCHAR(100) NULL,
    PhotoUrl NVARCHAR(500) NULL,
    PasswordHash NVARCHAR(255) NULL,
    
    -- Manager Reference (for team hierarchy)
    ManagerId INT NULL,
    ManagerName NVARCHAR(100) NULL,
    
    -- Assigned entities as JSON arrays (simplified approach)
    AssignedWorkshopOwnerIds NVARCHAR(MAX) NULL, -- JSON: [1, 2, 3]
    AssignedAreaIds NVARCHAR(MAX) NULL,          -- JSON: [1, 5, 8]
    AssignedBrandIds NVARCHAR(MAX) NULL,         -- JSON: [2, 4, 7]
    
    -- Permissions (19 boolean fields to avoid joins)
    CanViewOrders BIT NOT NULL DEFAULT 0,
    CanCreateOrders BIT NOT NULL DEFAULT 0,
    CanEditOrders BIT NOT NULL DEFAULT 0,
    CanDeleteOrders BIT NOT NULL DEFAULT 0,
    CanViewCustomers BIT NOT NULL DEFAULT 0,
    CanEditCustomers BIT NOT NULL DEFAULT 0,
    CanDeleteCustomers BIT NOT NULL DEFAULT 0,
    CanViewInvoices BIT NOT NULL DEFAULT 0,
    CanCreateInvoices BIT NOT NULL DEFAULT 0,
    CanEditInvoices BIT NOT NULL DEFAULT 0,
    CanManageTeam BIT NOT NULL DEFAULT 0,
    CanViewReports BIT NOT NULL DEFAULT 0,
    CanApproveOrders BIT NOT NULL DEFAULT 0,
    CanExportData BIT NOT NULL DEFAULT 0,
    CanSystemSettings BIT NOT NULL DEFAULT 0,
    CanUpdateOrderStatus BIT NOT NULL DEFAULT 0,
    CanUpdateInventory BIT NOT NULL DEFAULT 0,
    CanViewStock BIT NOT NULL DEFAULT 0,
    CanProcessShipments BIT NOT NULL DEFAULT 0,
    
    -- Timestamps
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Self-referencing FK for Manager
    CONSTRAINT FK_SalesPersons_Manager FOREIGN KEY (ManagerId) 
        REFERENCES SalesPersons(Id)
);

CREATE INDEX IX_SalesPersons_Email ON SalesPersons(Email);
CREATE INDEX IX_SalesPersons_Phone ON SalesPersons(Phone);
CREATE INDEX IX_SalesPersons_Role ON SalesPersons(Role);
CREATE INDEX IX_SalesPersons_Status ON SalesPersons(Status);
CREATE INDEX IX_SalesPersons_ManagerId ON SalesPersons(ManagerId);

PRINT 'SalesPersons table created successfully!';

PRINT 'All tables created successfully!';
