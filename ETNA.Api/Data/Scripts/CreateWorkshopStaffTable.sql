-- SQL Script to create WorkshopStaff table
-- Run this in SQL Server Management Studio or Azure Data Studio

-- Drop existing table if recreating
-- DROP TABLE IF EXISTS WorkshopStaff;

CREATE TABLE WorkshopStaff (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Staff Details
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhotoUrl NVARCHAR(500) NULL,
    
    -- Workshop Reference
    WorkshopOwnerId INT NOT NULL,
    
    -- Verification & Status
    IsPhoneVerified BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 0,
    RegistrationStatus INT NOT NULL DEFAULT 0,
    -- 0 = PendingPhoneVerification
    -- 1 = PendingOwnerApproval
    -- 2 = Approved
    -- 3 = Rejected
    -- 4 = Suspended
    
    -- Approval Details
    ApprovedAt DATETIME2 NULL,
    ApprovedByOwnerId INT NULL,
    RejectionReason NVARCHAR(500) NULL,
    
    -- Timestamps
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Foreign Key
    CONSTRAINT FK_WorkshopStaff_WorkshopOwner FOREIGN KEY (WorkshopOwnerId) 
        REFERENCES WorkshopOwners(Id)
);

-- Create indexes
CREATE INDEX IX_WorkshopStaff_Email ON WorkshopStaff(Email);
CREATE INDEX IX_WorkshopStaff_PhoneNumber ON WorkshopStaff(PhoneNumber);
CREATE INDEX IX_WorkshopStaff_WorkshopOwnerId ON WorkshopStaff(WorkshopOwnerId);
CREATE INDEX IX_WorkshopStaff_City ON WorkshopStaff(City);
CREATE INDEX IX_WorkshopStaff_RegistrationStatus ON WorkshopStaff(RegistrationStatus);
CREATE INDEX IX_WorkshopStaff_IsActive ON WorkshopStaff(IsActive);

PRINT 'WorkshopStaff table created successfully!';
