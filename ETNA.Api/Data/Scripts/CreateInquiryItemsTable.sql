-- Create InquiryItems Table
-- This table stores individual part items for each inquiry

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InquiryItems')
BEGIN
    CREATE TABLE InquiryItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InquiryId INT NOT NULL,
        PartName NVARCHAR(200) NOT NULL,
        PreferredBrand NVARCHAR(100) NOT NULL,
        Quantity INT NOT NULL,
        Remark NVARCHAR(500) NOT NULL,
        AudioUrl NVARCHAR(500) NULL,
        AudioDuration INT NULL,
        Image1Url NVARCHAR(500) NULL,
        Image2Url NVARCHAR(500) NULL,
        Image3Url NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        
        -- Foreign key constraint
        CONSTRAINT FK_InquiryItems_Inquiries FOREIGN KEY (InquiryId) REFERENCES Inquiries(Id) ON DELETE CASCADE,
        
        -- Check constraint for quantity
        CONSTRAINT CK_InquiryItems_Quantity CHECK (Quantity > 0)
    );
    
    -- Create index for better query performance
    CREATE INDEX IX_InquiryItems_InquiryId ON InquiryItems(InquiryId);
    
    PRINT 'InquiryItems table created successfully';
END
ELSE
BEGIN
    PRINT 'InquiryItems table already exists';
END
GO
