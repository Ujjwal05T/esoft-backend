-- Migration: Make RequestedByStaffId nullable in Inquiries table
-- This allows owners to create inquiries directly without a staff member

-- Check if the table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Inquiries')
BEGIN
    -- Alter the column to allow NULL
    ALTER TABLE Inquiries
    ALTER COLUMN RequestedByStaffId INT NULL;
    
    PRINT 'RequestedByStaffId column updated to allow NULL values';
END
ELSE
BEGIN
    PRINT 'Inquiries table does not exist. Please run CreateInquiriesTable.sql first.';
END
GO
