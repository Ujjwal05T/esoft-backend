-- SQL Script to create WorkshopOwners table (Updated with ETNA verification fields)
-- Run this in SQL Server Management Studio or Azure Data Studio

-- Drop existing table if recreating
-- DROP TABLE IF EXISTS WorkshopOwners;

CREATE TABLE WorkshopOwners (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Owner Details
    OwnerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    OwnerPhotoUrl NVARCHAR(500) NULL,
    
    -- Workshop Details
    WorkshopName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    TradeLicense NVARCHAR(100) NOT NULL,
    TradeLicenseDocumentUrl NVARCHAR(500) NULL,
    WorkshopPhotoUrl NVARCHAR(500) NULL,
    
    -- ETNA Verification Details
    ETNAVerifierName NVARCHAR(100) NULL,
    ETNAVerifierPhone NVARCHAR(20) NULL,
    
    -- Verification & Status
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsETNAVerified BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 0,
    RegistrationStatus INT NOT NULL DEFAULT 0,
    -- 0 = PendingEmailVerification
    -- 1 = PendingETNAVerification
    -- 2 = PendingPhotoUpload
    -- 3 = Active
    -- 4 = Rejected
    -- 5 = Suspended
    
    -- Timestamps
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    ETNAVerifiedAt DATETIME2 NULL,
    ActivatedAt DATETIME2 NULL
);

-- Create indexes
CREATE INDEX IX_WorkshopOwners_Email ON WorkshopOwners(Email);
CREATE INDEX IX_WorkshopOwners_City ON WorkshopOwners(City);
CREATE INDEX IX_WorkshopOwners_IsActive ON WorkshopOwners(IsActive);
CREATE INDEX IX_WorkshopOwners_RegistrationStatus ON WorkshopOwners(RegistrationStatus);

PRINT 'WorkshopOwners table created successfully!';
