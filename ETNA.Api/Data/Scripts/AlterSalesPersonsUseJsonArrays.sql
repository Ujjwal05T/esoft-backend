-- SQL Script to alter SalesPersons table to use JSON arrays instead of junction tables
-- Run this in SQL Server Management Studio or Azure Data Studio

-- =====================================================
-- Add JSON array columns to SalesPersons table
-- =====================================================
ALTER TABLE SalesPersons
ADD AssignedWorkshopOwnerIds NVARCHAR(MAX) NULL,
    AssignedAreaIds NVARCHAR(MAX) NULL,
    AssignedBrandIds NVARCHAR(MAX) NULL;

PRINT 'Added JSON array columns to SalesPersons table!';

-- =====================================================
-- Drop junction tables (if they exist)
-- =====================================================
IF OBJECT_ID('SalesPersonWorkshopOwners', 'U') IS NOT NULL
    DROP TABLE SalesPersonWorkshopOwners;

IF OBJECT_ID('SalesPersonAreas', 'U') IS NOT NULL
    DROP TABLE SalesPersonAreas;

IF OBJECT_ID('SalesPersonBrands', 'U') IS NOT NULL
    DROP TABLE SalesPersonBrands;

PRINT 'Dropped junction tables!';

PRINT 'Migration completed successfully!';
