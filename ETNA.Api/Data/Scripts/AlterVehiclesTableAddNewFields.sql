-- ================================================
-- ALTER TABLE: Vehicles - Add new columns
-- ================================================

-- Add Specs column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'Specs')
BEGIN
    ALTER TABLE Vehicles ADD Specs NVARCHAR(100) NULL;
    PRINT 'Added Specs column to Vehicles table';
END

-- Add RegistrationName column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'RegistrationName')
BEGIN
    ALTER TABLE Vehicles ADD RegistrationName NVARCHAR(100) NULL;
    PRINT 'Added RegistrationName column to Vehicles table';
END

-- Add Email column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'Email')
BEGIN
    ALTER TABLE Vehicles ADD Email NVARCHAR(255) NULL;
    PRINT 'Added Email column to Vehicles table';
END

-- Add GstNumber column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'GstNumber')
BEGIN
    ALTER TABLE Vehicles ADD GstNumber NVARCHAR(20) NULL;
    PRINT 'Added GstNumber column to Vehicles table';
END

-- Add InsuranceProvider column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'InsuranceProvider')
BEGIN
    ALTER TABLE Vehicles ADD InsuranceProvider NVARCHAR(100) NULL;
    PRINT 'Added InsuranceProvider column to Vehicles table';
END

PRINT 'Vehicles table alterations completed';
GO
