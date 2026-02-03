using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public QuoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateQuoteAsync(Quote quote)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO Quotes (
                QuoteNumber, InquiryId, VehicleId, WorkshopOwnerId, CreatedByStaffId,
                PackingCharges, ForwardingCharges, ShippingCharges, TotalAmount,
                Status, CreatedAt, ExpiresAt
            )
            VALUES (
                @QuoteNumber, @InquiryId, @VehicleId, @WorkshopOwnerId, @CreatedByStaffId,
                @PackingCharges, @ForwardingCharges, @ShippingCharges, @TotalAmount,
                @Status, @CreatedAt, @ExpiresAt
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            quote.QuoteNumber,
            quote.InquiryId,
            quote.VehicleId,
            quote.WorkshopOwnerId,
            quote.CreatedByStaffId,
            quote.PackingCharges,
            quote.ForwardingCharges,
            quote.ShippingCharges,
            quote.TotalAmount,
            quote.Status,
            quote.CreatedAt,
            quote.ExpiresAt
        });
    }

    public async Task<int> CreateQuoteItemAsync(QuoteItem item)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO QuoteItems (
                QuoteId, InquiryItemId, PartName, PartNumber, Brand, Description,
                Quantity, Mrp, UnitPrice, Availability, EstimatedDelivery, CreatedAt
            )
            VALUES (
                @QuoteId, @InquiryItemId, @PartName, @PartNumber, @Brand, @Description,
                @Quantity, @Mrp, @UnitPrice, @Availability, @EstimatedDelivery, @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            item.QuoteId,
            item.InquiryItemId,
            item.PartName,
            item.PartNumber,
            item.Brand,
            item.Description,
            item.Quantity,
            item.Mrp,
            item.UnitPrice,
            item.Availability,
            item.EstimatedDelivery,
            item.CreatedAt
        });
    }

    public async Task<Quote?> GetQuoteByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Quotes WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Quote>(sql, new { Id = id });
    }

    public async Task<List<Quote>> GetAllQuotesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Quotes ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Quote>(sql);
        return result.ToList();
    }

    public async Task<List<Quote>> GetQuotesByInquiryIdAsync(int inquiryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Quotes WHERE InquiryId = @InquiryId ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Quote>(sql, new { InquiryId = inquiryId });
        return result.ToList();
    }

    public async Task<List<Quote>> GetQuotesByVehicleIdAsync(int vehicleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Quotes WHERE VehicleId = @VehicleId ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Quote>(sql, new { VehicleId = vehicleId });
        return result.ToList();
    }

    public async Task<List<Quote>> GetQuotesByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Quotes WHERE WorkshopOwnerId = @WorkshopOwnerId ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Quote>(sql, new { WorkshopOwnerId = workshopOwnerId });
        return result.ToList();
    }

    public async Task<List<QuoteItem>> GetQuoteItemsByQuoteIdAsync(int quoteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM QuoteItems WHERE QuoteId = @QuoteId ORDER BY CreatedAt";
        var result = await connection.QueryAsync<QuoteItem>(sql, new { QuoteId = quoteId });
        return result.ToList();
    }

    public async Task<bool> UpdateQuoteStatusAsync(int id, string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Quotes SET
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteQuoteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Quotes WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<string> GenerateQuoteNumberAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string countSql = "SELECT COUNT(*) FROM Quotes WHERE WorkshopOwnerId = @WorkshopOwnerId";
        var count = await connection.ExecuteScalarAsync<int>(countSql, new { WorkshopOwnerId = workshopOwnerId });

        // Format: ET/QT/YY-YY/XXXXX
        var currentYear = DateTime.UtcNow.Year % 100;
        var nextYear = (currentYear + 1) % 100;
        var sequenceNumber = (count + 1).ToString("D5");

        return $"ET/QT/{currentYear:D2}-{nextYear:D2}/{sequenceNumber}";
    }
}
