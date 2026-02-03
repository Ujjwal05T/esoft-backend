using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

public class InquiryRepository : IInquiryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InquiryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateInquiryAsync(Inquiry inquiry)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO Inquiries (
                VehicleId, VehicleVisitId, WorkshopOwnerId, RequestedByStaffId,
                InquiryNumber, JobCategory, Status, AssignedToId, AssignedToName,
                PlacedDate, CreatedAt
            )
            VALUES (
                @VehicleId, @VehicleVisitId, @WorkshopOwnerId, @RequestedByStaffId,
                @InquiryNumber, @JobCategory, @Status, @AssignedToId, @AssignedToName,
                @PlacedDate, @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            inquiry.VehicleId,
            inquiry.VehicleVisitId,
            inquiry.WorkshopOwnerId,
            inquiry.RequestedByStaffId,
            inquiry.InquiryNumber,
            inquiry.JobCategory,
            Status = inquiry.Status.ToString().ToLower(),
            inquiry.AssignedToId,
            inquiry.AssignedToName,
            inquiry.PlacedDate,
            inquiry.CreatedAt
        });
    }

    public async Task<int> CreateInquiryItemAsync(InquiryItem item)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO InquiryItems (
                InquiryId, PartName, PreferredBrand, Quantity, Remark,
                AudioUrl, AudioDuration, Image1Url, Image2Url, Image3Url, CreatedAt
            )
            VALUES (
                @InquiryId, @PartName, @PreferredBrand, @Quantity, @Remark,
                @AudioUrl, @AudioDuration, @Image1Url, @Image2Url, @Image3Url, @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, item);
    }

    public async Task<List<Inquiry>> GetAllInquiriesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inquiries ORDER BY PlacedDate DESC";
        var result = await connection.QueryAsync<Inquiry>(sql);
        return result.ToList();
    }

    public async Task<Inquiry?> GetInquiryByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inquiries WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Inquiry>(sql, new { Id = id });
    }

    public async Task<List<InquiryItem>> GetInquiryItemsByInquiryIdAsync(int inquiryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM InquiryItems WHERE InquiryId = @InquiryId ORDER BY CreatedAt";
        var result = await connection.QueryAsync<InquiryItem>(sql, new { InquiryId = inquiryId });
        return result.ToList();
    }

    public async Task<List<Inquiry>> GetInquiriesByVehicleIdAsync(int vehicleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inquiries WHERE VehicleId = @VehicleId ORDER BY PlacedDate DESC";
        var result = await connection.QueryAsync<Inquiry>(sql, new { VehicleId = vehicleId });
        return result.ToList();
    }

    public async Task<List<Inquiry>> GetInquiriesByVehicleVisitIdAsync(int vehicleVisitId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inquiries WHERE VehicleVisitId = @VehicleVisitId ORDER BY PlacedDate DESC";
        var result = await connection.QueryAsync<Inquiry>(sql, new { VehicleVisitId = vehicleVisitId });
        return result.ToList();
    }

    public async Task<List<Inquiry>> GetInquiriesByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inquiries WHERE WorkshopOwnerId = @WorkshopOwnerId ORDER BY PlacedDate DESC";
        var result = await connection.QueryAsync<Inquiry>(sql, new { WorkshopOwnerId = workshopOwnerId });
        return result.ToList();
    }

    public async Task<List<Inquiry>> GetInquiriesByStatusAsync(int workshopOwnerId, InquiryStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Inquiries 
            WHERE WorkshopOwnerId = @WorkshopOwnerId AND Status = @Status 
            ORDER BY PlacedDate DESC";
        var result = await connection.QueryAsync<Inquiry>(sql, new { WorkshopOwnerId = workshopOwnerId, Status = status.ToString().ToLower() });
        return result.ToList();
    }

    public async Task<bool> UpdateInquiryStatusAsync(int id, InquiryStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            UPDATE Inquiries SET
                Status = @Status,
                UpdatedAt = @UpdatedAt";

        // Add specific date fields based on status
        if (status == InquiryStatus.Closed)
        {
            sql += ", ClosedDate = @ClosedDate";
        }
        else if (status == InquiryStatus.Declined)
        {
            sql += ", DeclinedDate = @DeclinedDate";
        }

        sql += " WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status.ToString().ToLower(),
            UpdatedAt = DateTime.UtcNow,
            ClosedDate = status == InquiryStatus.Closed ? DateTime.UtcNow : (DateTime?)null,
            DeclinedDate = status == InquiryStatus.Declined ? DateTime.UtcNow : (DateTime?)null
        });

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateInquiryAsync(Inquiry inquiry)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE Inquiries SET
                JobCategory = @JobCategory,
                Status = @Status,
                ClosedDate = @ClosedDate,
                DeclinedDate = @DeclinedDate,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        inquiry.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            inquiry.Id,
            inquiry.JobCategory,
            Status = inquiry.Status.ToString().ToLower(),
            inquiry.ClosedDate,
            inquiry.DeclinedDate,
            inquiry.UpdatedAt
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteInquiryAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Inquiries WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<string> GenerateInquiryNumberAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Get the count of inquiries for this workshop owner
        const string countSql = "SELECT COUNT(*) FROM Inquiries WHERE WorkshopOwnerId = @WorkshopOwnerId";
        var count = await connection.ExecuteScalarAsync<int>(countSql, new { WorkshopOwnerId = workshopOwnerId });
        
        // Generate inquiry number in format: ET/SALES/YY-YY/XXXXX
        var currentYear = DateTime.UtcNow.Year % 100; // Get last 2 digits
        var nextYear = (currentYear + 1) % 100;
        var sequenceNumber = (count + 1).ToString("D5");
        
        return $"ET/SALES/{currentYear:D2}-{nextYear:D2}/{sequenceNumber}";
    }

    public async Task<int> GetInquiryCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Inquiries WHERE WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }
}
