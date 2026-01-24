-- ================================================
-- CREATE TABLE: VehicleVisits
-- Tracks vehicle visits with gate in/out details
-- ================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VehicleVisits' AND xtype='U')
BEGIN
    CREATE TABLE VehicleVisits (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Foreign Keys
        VehicleId INT NOT NULL,
        WorkshopOwnerId INT NOT NULL,
        
        -- Visit Status (0=In, 1=Out)
        Status INT NOT NULL DEFAULT 0,
        
        -- ========== GATE IN DETAILS ==========
        GateInDateTime DATETIME2 NOT NULL,
        GateInDriverName NVARCHAR(100) NOT NULL,
        GateInDriverContact NVARCHAR(20) NOT NULL,
        GateInOdometerReading NVARCHAR(20) NULL,
        GateInFuelLevel INT NULL,  -- 0-100 percentage
        GateInProblemShared NVARCHAR(MAX) NULL,
        GateInProblemAudioUrl NVARCHAR(500) NULL,
        GateInImages NVARCHAR(MAX) NULL,  -- JSON array of image URLs
        
        -- ========== GATE OUT DETAILS ==========
        GateOutDateTime DATETIME2 NULL,
        GateOutDriverName NVARCHAR(100) NULL,
        GateOutDriverContact NVARCHAR(20) NULL,
        GateOutOdometerReading NVARCHAR(20) NULL,
        GateOutFuelLevel INT NULL,  -- 0-100 percentage
        GateOutImages NVARCHAR(MAX) NULL,  -- JSON array of image URLs
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        -- Foreign Key Constraints
        CONSTRAINT FK_VehicleVisits_Vehicles FOREIGN KEY (VehicleId)
            REFERENCES Vehicles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_VehicleVisits_WorkshopOwners FOREIGN KEY (WorkshopOwnerId)
            REFERENCES WorkshopOwners(Id),
            
        -- Check constraint for fuel level range
        CONSTRAINT CHK_VehicleVisits_GateInFuelLevel CHECK (GateInFuelLevel IS NULL OR (GateInFuelLevel >= 0 AND GateInFuelLevel <= 100)),
        CONSTRAINT CHK_VehicleVisits_GateOutFuelLevel CHECK (GateOutFuelLevel IS NULL OR (GateOutFuelLevel >= 0 AND GateOutFuelLevel <= 100))
    );
    
    -- Create indexes for common queries
    
    -- Index on VehicleId for getting all visits for a vehicle
    CREATE INDEX IX_VehicleVisits_VehicleId ON VehicleVisits(VehicleId);
    
    -- Index on WorkshopOwnerId for filtering by workshop
    CREATE INDEX IX_VehicleVisits_WorkshopOwnerId ON VehicleVisits(WorkshopOwnerId);
    
    -- Index on Status for filtering active/completed visits
    CREATE INDEX IX_VehicleVisits_Status ON VehicleVisits(Status);
    
    -- Composite index for common query: vehicles currently in a workshop
    CREATE INDEX IX_VehicleVisits_Workshop_Status ON VehicleVisits(WorkshopOwnerId, Status)
        INCLUDE (VehicleId, GateInDateTime);
    
    -- Index for getting latest visit for a vehicle
    CREATE INDEX IX_VehicleVisits_Vehicle_GateIn ON VehicleVisits(VehicleId, GateInDateTime DESC);
    
    PRINT 'VehicleVisits table created successfully';
END
ELSE
BEGIN
    PRINT 'VehicleVisits table already exists';
END
GO
