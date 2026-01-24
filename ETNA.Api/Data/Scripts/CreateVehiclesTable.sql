-- ================================================
-- CREATE TABLE: Vehicles
-- ================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vehicles' AND xtype='U')
BEGIN
    CREATE TABLE Vehicles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Vehicle Details
        PlateNumber NVARCHAR(20) NOT NULL,
        Brand NVARCHAR(50) NULL,
        Model NVARCHAR(50) NULL,
        Year INT NULL,
        Variant NVARCHAR(50) NULL,
        ChassisNumber NVARCHAR(50) NULL,
        
        -- Owner Details
        OwnerName NVARCHAR(100) NOT NULL,
        ContactNumber NVARCHAR(15) NOT NULL,
        
        -- Service Details
        OdometerReading NVARCHAR(20) NULL,
        Observations NVARCHAR(MAX) NULL,
        ObservationsAudioUrl NVARCHAR(500) NULL,
        
        -- Workshop Association
        WorkshopOwnerId INT NOT NULL,
        
        -- Status (0=Active, 1=InService, 2=Completed, 3=Archived)
        Status INT NOT NULL DEFAULT 0,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        -- Foreign Key
        CONSTRAINT FK_Vehicles_WorkshopOwners FOREIGN KEY (WorkshopOwnerId)
            REFERENCES WorkshopOwners(Id)
    );
    
    -- Create index on PlateNumber for faster lookups
    CREATE INDEX IX_Vehicles_PlateNumber ON Vehicles(PlateNumber);
    
    -- Create index on WorkshopOwnerId for filtering by workshop
    CREATE INDEX IX_Vehicles_WorkshopOwnerId ON Vehicles(WorkshopOwnerId);
    
    PRINT 'Vehicles table created successfully';
END
ELSE
BEGIN
    PRINT 'Vehicles table already exists';
END
GO
