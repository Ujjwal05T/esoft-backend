-- Create Quotes Table
-- This table stores quotes created by sales persons in response to inquiries

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Quotes')
BEGIN
    CREATE TABLE Quotes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        QuoteNumber NVARCHAR(50) NOT NULL UNIQUE,
        InquiryId INT NOT NULL,
        VehicleId INT NOT NULL,
        WorkshopOwnerId INT NOT NULL,
        CreatedByStaffId INT NULL,
        PackingCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        ForwardingCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        ShippingCharges DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NULL,
        ExpiresAt DATETIME NULL,

        -- Foreign key constraints
        CONSTRAINT FK_Quotes_Inquiries FOREIGN KEY (InquiryId) REFERENCES Inquiries(Id),
        CONSTRAINT FK_Quotes_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id),
        CONSTRAINT FK_Quotes_WorkshopOwners FOREIGN KEY (WorkshopOwnerId) REFERENCES WorkshopOwners(Id),
        CONSTRAINT FK_Quotes_WorkshopStaff FOREIGN KEY (CreatedByStaffId) REFERENCES WorkshopStaff(Id),

        -- Check constraint for status
        CONSTRAINT CK_Quotes_Status CHECK (Status IN ('pending', 'approved', 'rejected'))
    );

    -- Create indexes for better query performance
    CREATE INDEX IX_Quotes_QuoteNumber ON Quotes(QuoteNumber);
    CREATE INDEX IX_Quotes_InquiryId ON Quotes(InquiryId);
    CREATE INDEX IX_Quotes_VehicleId ON Quotes(VehicleId);
    CREATE INDEX IX_Quotes_WorkshopOwnerId ON Quotes(WorkshopOwnerId);
    CREATE INDEX IX_Quotes_Status ON Quotes(Status);
    CREATE INDEX IX_Quotes_CreatedAt ON Quotes(CreatedAt DESC);

    PRINT 'Quotes table created successfully';
END
ELSE
BEGIN
    PRINT 'Quotes table already exists';
END
GO
