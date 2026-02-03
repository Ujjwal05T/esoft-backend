-- Create QuoteItems Table
-- This table stores individual part line items within a quote
-- Depends on: Quotes, InquiryItems — run CreateQuotesTable.sql first

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuoteItems')
BEGIN
    CREATE TABLE QuoteItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        QuoteId INT NOT NULL,
        InquiryItemId INT NOT NULL,
        PartName NVARCHAR(255) NOT NULL,
        PartNumber NVARCHAR(100) NOT NULL DEFAULT '',
        Brand NVARCHAR(100) NOT NULL DEFAULT '',
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        Quantity INT NOT NULL,
        Mrp DECIMAL(18,2) NOT NULL DEFAULT 0,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Availability NVARCHAR(50) NOT NULL DEFAULT 'in_stock',
        EstimatedDelivery DATE NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

        -- Foreign key constraints
        CONSTRAINT FK_QuoteItems_Quotes FOREIGN KEY (QuoteId) REFERENCES Quotes(Id) ON DELETE CASCADE,
        CONSTRAINT FK_QuoteItems_InquiryItems FOREIGN KEY (InquiryItemId) REFERENCES InquiryItems(Id),

        -- Check constraint for availability
        CONSTRAINT CK_QuoteItems_Availability CHECK (Availability IN ('in_stock', 'out_of_stock', 'on_order', 'discontinued'))
    );

    -- Create indexes for better query performance
    CREATE INDEX IX_QuoteItems_QuoteId ON QuoteItems(QuoteId);
    CREATE INDEX IX_QuoteItems_InquiryItemId ON QuoteItems(InquiryItemId);

    PRINT 'QuoteItems table created successfully';
END
ELSE
BEGIN
    PRINT 'QuoteItems table already exists';
END
GO
