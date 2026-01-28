-- Fix WorkshopStaff Email column to be nullable and allow multiple NULLs
-- This allows staff members to be created without email addresses

-- Step 1: Drop the existing UNIQUE constraint on Email
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name 
FROM sys.key_constraints 
WHERE type = 'UQ' 
  AND parent_object_id = OBJECT_ID('WorkshopStaff')
  AND COL_NAME(parent_object_id, parent_column_id) = 'Email';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE WorkshopStaff DROP CONSTRAINT ' + @ConstraintName);
    PRINT 'Dropped UNIQUE constraint on Email column';
END

-- Step 2: Make Email column nullable
ALTER TABLE WorkshopStaff
ALTER COLUMN Email NVARCHAR(255) NULL;

PRINT 'Email column is now nullable';

-- Step 3: Create a filtered unique index (allows multiple NULLs but ensures unique non-NULL values)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_WorkshopStaff_Email_NotNull')
BEGIN
    CREATE UNIQUE INDEX UQ_WorkshopStaff_Email_NotNull 
    ON WorkshopStaff(Email) 
    WHERE Email IS NOT NULL;
    
    PRINT 'Created filtered unique index on Email (allows multiple NULLs)';
END

PRINT 'WorkshopStaff Email column migration completed successfully!';
