-- ================================================
-- ALTER TABLE: Add Contact Person and GST fields
-- Run this script to add new columns to existing table
-- ================================================

-- Add Contact Person Name (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkshopOwners') 
    AND name = 'ContactPersonName'
)
BEGIN
    ALTER TABLE WorkshopOwners
    ADD ContactPersonName NVARCHAR(100) NULL;
    PRINT 'Added ContactPersonName column';
END
GO

-- Add Contact Person Mobile (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkshopOwners') 
    AND name = 'ContactPersonMobile'
)
BEGIN
    ALTER TABLE WorkshopOwners
    ADD ContactPersonMobile NVARCHAR(15) NULL;
    PRINT 'Added ContactPersonMobile column';
END
GO

-- Add GST Number (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('WorkshopOwners') 
    AND name = 'GSTNumber'
)
BEGIN
    ALTER TABLE WorkshopOwners
    ADD GSTNumber NVARCHAR(20) NULL;
    PRINT 'Added GSTNumber column';
END
GO

-- Verify the columns were added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'WorkshopOwners'
AND COLUMN_NAME IN ('ContactPersonName', 'ContactPersonMobile', 'GSTNumber')
ORDER BY ORDINAL_POSITION;
GO

PRINT 'ALTER TABLE script completed successfully!';
