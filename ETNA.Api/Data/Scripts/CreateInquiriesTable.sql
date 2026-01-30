-- Create Inquiries Table
-- This table stores part request inquiries made during vehicle visits

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inquiries')
BEGIN
    CREATE TABLE Inquiries (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehicleId INT NOT NULL,
        VehicleVisitId INT NULL,
        WorkshopOwnerId INT NOT NULL,
        RequestedByStaffId INT NULL,
        InquiryNumber NVARCHAR(50) NOT NULL UNIQUE,
        JobCategory NVARCHAR(100) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'open',
        PlacedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ClosedDate DATETIME NULL,
        DeclinedDate DATETIME NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NULL,
        
        -- Foreign key constraints
        CONSTRAINT FK_Inquiries_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id),
        CONSTRAINT FK_Inquiries_VehicleVisits FOREIGN KEY (VehicleVisitId) REFERENCES VehicleVisits(Id),
        CONSTRAINT FK_Inquiries_WorkshopOwners FOREIGN KEY (WorkshopOwnerId) REFERENCES WorkshopOwners(Id),
        CONSTRAINT FK_Inquiries_WorkshopStaff FOREIGN KEY (RequestedByStaffId) REFERENCES WorkshopStaff(Id),
        
        -- Check constraint for status
        CONSTRAINT CK_Inquiries_Status CHECK (Status IN ('open', 'closed', 'approved', 'requested', 'declined'))
    );
    
    -- Create indexes for better query performance
    CREATE INDEX IX_Inquiries_VehicleId ON Inquiries(VehicleId);
    CREATE INDEX IX_Inquiries_VehicleVisitId ON Inquiries(VehicleVisitId);
    CREATE INDEX IX_Inquiries_WorkshopOwnerId ON Inquiries(WorkshopOwnerId);
    CREATE INDEX IX_Inquiries_Status ON Inquiries(Status);
    CREATE INDEX IX_Inquiries_PlacedDate ON Inquiries(PlacedDate DESC);
    
    PRINT 'Inquiries table created successfully';
END
ELSE
BEGIN
    PRINT 'Inquiries table already exists';
END
GO
