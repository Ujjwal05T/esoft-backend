-- SQL Script to add new columns to WorkshopStaff table
-- Based on frontend AddStaffOverlay and EditStaffOverlay requirements
-- Run this in SQL Server Management Studio or Azure Data Studio

-- =====================================================
-- ADD STAFF DETAIL COLUMNS
-- =====================================================

-- 1. Add Address column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'Address')
BEGIN
    ALTER TABLE WorkshopStaff ADD Address NVARCHAR(500) NULL;
    PRINT 'Added Address column';
END
GO

-- 2. Add AadhaarNumber column (12-digit Indian ID)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'AadhaarNumber')
BEGIN
    ALTER TABLE WorkshopStaff ADD AadhaarNumber NVARCHAR(14) NULL;
    PRINT 'Added AadhaarNumber column';
END
GO

-- 3. Add JobCategories column (JSON array stored as string)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'JobCategories')
BEGIN
    ALTER TABLE WorkshopStaff ADD JobCategories NVARCHAR(1000) NULL;
    PRINT 'Added JobCategories column';
END
GO

-- =====================================================
-- ADD PERMISSION COLUMNS (6 boolean columns)
-- =====================================================

-- 4. CanApproveVehicles - Can approve vehicle entries/exits
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanApproveVehicles')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanApproveVehicles BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanApproveVehicles column';
END
GO

-- 5. CanApproveInquiries - Can approve customer inquiries
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanApproveInquiries')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanApproveInquiries BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanApproveInquiries column';
END
GO

-- 6. CanGenerateEstimates - Can generate cost estimates/quotes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanGenerateEstimates')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanGenerateEstimates BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanGenerateEstimates column';
END
GO

-- 7. CanCreateJobCard - Can create new job cards
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanCreateJobCard')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanCreateJobCard BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanCreateJobCard column';
END
GO

-- 8. CanApproveDisputes - Can approve/handle customer disputes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanApproveDisputes')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanApproveDisputes BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanApproveDisputes column';
END
GO

-- 9. CanApproveQuotesPayments - Can approve quotes and process payments
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'WorkshopStaff') AND name = 'CanApproveQuotesPayments')
BEGIN
    ALTER TABLE WorkshopStaff ADD CanApproveQuotesPayments BIT NOT NULL DEFAULT 0;
    PRINT 'Added CanApproveQuotesPayments column';
END
GO

-- =====================================================
-- CREATE INDEXES
-- =====================================================

-- Index on AadhaarNumber for lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkshopStaff_AadhaarNumber')
BEGIN
    CREATE INDEX IX_WorkshopStaff_AadhaarNumber ON WorkshopStaff(AadhaarNumber);
    PRINT 'Created index IX_WorkshopStaff_AadhaarNumber';
END
GO

PRINT '========================================';
PRINT 'WorkshopStaff table alterations complete!';
PRINT '========================================';
