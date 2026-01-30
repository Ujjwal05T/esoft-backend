-- Create JobCards table
CREATE TABLE JobCards (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VehicleId INT NOT NULL,
    VehicleVisitId INT NULL,
    WorkshopOwnerId INT NOT NULL,
    JobCategory NVARCHAR(100) NOT NULL,
    AssignedStaffId INT NULL,
    AssignedStaffName NVARCHAR(255) NULL,
    Remark NVARCHAR(MAX) NULL,
    AudioUrl NVARCHAR(500) NULL,
    Images NVARCHAR(MAX) NULL, -- JSON array of image URLs
    Videos NVARCHAR(MAX) NULL, -- JSON array of video URLs
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Priority NVARCHAR(20) NULL DEFAULT 'Normal',
    EstimatedCost DECIMAL(10,2) NULL,
    ActualCost DECIMAL(10,2) NULL,
    EstimatedDuration INT NULL, -- in minutes
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Foreign Keys
    CONSTRAINT FK_JobCards_Vehicle FOREIGN KEY (VehicleId) 
        REFERENCES Vehicles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_JobCards_VehicleVisit FOREIGN KEY (VehicleVisitId) 
        REFERENCES VehicleVisits(Id) ON DELETE SET NULL,
    CONSTRAINT FK_JobCards_WorkshopOwner FOREIGN KEY (WorkshopOwnerId) 
        REFERENCES WorkshopOwners(Id),
    CONSTRAINT FK_JobCards_WorkshopStaff FOREIGN KEY (AssignedStaffId) 
        REFERENCES WorkshopStaff(Id) ON DELETE SET NULL
);

-- Create indexes for better query performance
CREATE INDEX IX_JobCards_VehicleId ON JobCards(VehicleId);
CREATE INDEX IX_JobCards_VehicleVisitId ON JobCards(VehicleVisitId);
CREATE INDEX IX_JobCards_WorkshopOwnerId ON JobCards(WorkshopOwnerId);
CREATE INDEX IX_JobCards_AssignedStaffId ON JobCards(AssignedStaffId);
CREATE INDEX IX_JobCards_Status ON JobCards(Status);
CREATE INDEX IX_JobCards_CreatedAt ON JobCards(CreatedAt DESC);

-- Insert sample data
INSERT INTO JobCards (VehicleId, WorkshopOwnerId, JobCategory, AssignedStaffId, AssignedStaffName, Remark, Status, Priority, EstimatedCost, CreatedAt)
VALUES 
    (1, 1, 'General Service', NULL, NULL, 'Regular maintenance check', 'Pending', 'Normal', 2500.00, GETUTCDATE()),
    (1, 1, 'Engine', NULL, NULL, 'Engine oil change required', 'InProgress', 'High', 3500.00, DATEADD(DAY, -2, GETUTCDATE())),
    (2, 1, 'Brake System', NULL, NULL, 'Brake pads replacement', 'Completed', 'High', 4500.00, DATEADD(DAY, -5, GETUTCDATE()));
