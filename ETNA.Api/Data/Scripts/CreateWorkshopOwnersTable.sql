-- SQL Script to create/update WorkshopOwners table
-- Run this in SQL Server Management Studio or Azure Data Studio

-- Drop existing table if recreating (CAUTION: This will delete all data)
-- DROP TABLE IF EXISTS WorkshopOwners;

CREATE TABLE WorkshopOwners (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Owner Details
    OwnerName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(255) NULL,
    AadhaarNumber NVARCHAR(20) NOT NULL,
    PasswordHash NVARCHAR(255) NULL,
    OwnerPhotoUrl NVARCHAR(500) NULL,
    
    -- Workshop Details
    WorkshopName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NOT NULL,
    Landmark NVARCHAR(200) NULL,
    PinCode NVARCHAR(10) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    TradeLicenseDocumentUrl NVARCHAR(500) NULL,
    WorkshopPhotoUrl NVARCHAR(500) NULL,
    
    -- Source tracking
    Source NVARCHAR(20) NOT NULL DEFAULT 'app', -- 'app', 'whatsapp', 'phone_call'
    
    -- ETNA Verification Details
    ETNAVerifierName NVARCHAR(100) NULL,
    ETNAVerifierPhone NVARCHAR(20) NULL,
    
    -- Verification Status
    IsPhoneVerified BIT NOT NULL DEFAULT 0,
    IsETNAVerified BIT NOT NULL DEFAULT 0,
    
    -- Registration Status
    -- 0 = Pending
    -- 1 = UnderReview
    -- 2 = Active
    -- 3 = Rejected
    RegistrationStatus INT NOT NULL DEFAULT 0,
    
    -- Timestamps
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    ETNAVerifiedAt DATETIME2 NULL,
    ActivatedAt DATETIME2 NULL
);

-- Create indexes for faster queries
CREATE INDEX IX_WorkshopOwners_PhoneNumber ON WorkshopOwners(PhoneNumber);
CREATE INDEX IX_WorkshopOwners_Email ON WorkshopOwners(Email);
CREATE INDEX IX_WorkshopOwners_City ON WorkshopOwners(City);
CREATE INDEX IX_WorkshopOwners_RegistrationStatus ON WorkshopOwners(RegistrationStatus);
CREATE INDEX IX_WorkshopOwners_Source ON WorkshopOwners(Source);
CREATE INDEX IX_WorkshopOwners_CreatedAt ON WorkshopOwners(CreatedAt DESC);

PRINT 'WorkshopOwners table created successfully!';
GO

-- If table already exists and you want to add new columns, use this instead:
/*
-- Add new columns to existing table
ALTER TABLE WorkshopOwners ADD Landmark NVARCHAR(200) NULL;
ALTER TABLE WorkshopOwners ADD PinCode NVARCHAR(10) NOT NULL DEFAULT '';
ALTER TABLE WorkshopOwners ADD Source NVARCHAR(20) NOT NULL DEFAULT 'app';
ALTER TABLE WorkshopOwners ADD AadhaarNumber NVARCHAR(20) NOT NULL DEFAULT '';

-- Create new indexes
CREATE INDEX IX_WorkshopOwners_Source ON WorkshopOwners(Source);
CREATE INDEX IX_WorkshopOwners_CreatedAt ON WorkshopOwners(CreatedAt DESC);
*/
