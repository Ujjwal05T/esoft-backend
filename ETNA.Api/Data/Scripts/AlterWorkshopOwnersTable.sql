-- SQL Script to ALTER existing WorkshopOwners table with new columns
-- Run this if you already have the table created

-- Add ETNA Verification columns
ALTER TABLE WorkshopOwners ADD ETNAVerifierName NVARCHAR(100) NULL;
ALTER TABLE WorkshopOwners ADD ETNAVerifierPhone NVARCHAR(20) NULL;
ALTER TABLE WorkshopOwners ADD IsETNAVerified BIT NOT NULL DEFAULT 0;
ALTER TABLE WorkshopOwners ADD ETNAVerifiedAt DATETIME2 NULL;
ALTER TABLE WorkshopOwners ADD ActivatedAt DATETIME2 NULL;

-- Add index for RegistrationStatus
CREATE INDEX IX_WorkshopOwners_RegistrationStatus ON WorkshopOwners(RegistrationStatus);

PRINT 'WorkshopOwners table updated successfully!';
