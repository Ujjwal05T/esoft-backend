using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for WorkshopStaff data access using Dapper
/// </summary>
public class WorkshopStaffRepository : IWorkshopStaffRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WorkshopStaffRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<WorkshopStaff> CreateAsync(WorkshopStaff staff)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO WorkshopStaff (
                Name, PhoneNumber, Email, PasswordHash, Address, PhotoUrl,
                WorkshopId, WorkshopOwnerId, City, Role, JobCategories,
                CanApproveVehicles, CanApproveInquiries, CanGenerateEstimates,
                CanCreateJobCard, CanApproveDisputes, CanApproveQuotesPayments,
                IsActive, IsPhoneVerified, RegistrationStatus, CreatedAt
            )
            OUTPUT INSERTED.*
            VALUES (
                @Name, @PhoneNumber, @Email, @PasswordHash, @Address, @PhotoUrl,
                @WorkshopId, @WorkshopOwnerId, @City, @Role, @JobCategories,
                @CanApproveVehicles, @CanApproveInquiries, @CanGenerateEstimates,
                @CanCreateJobCard, @CanApproveDisputes, @CanApproveQuotesPayments,
                @IsActive, @IsPhoneVerified, @RegistrationStatus, @CreatedAt
            )";

        var result = await connection.QuerySingleAsync<WorkshopStaff>(sql, staff);
        return result;
    }

    public async Task<WorkshopStaff?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<WorkshopStaff>(sql, new { Id = id });
    }

    public async Task<WorkshopStaff?> GetByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE PhoneNumber = @PhoneNumber";
        return await connection.QuerySingleOrDefaultAsync<WorkshopStaff>(sql, new { PhoneNumber = phoneNumber });
    }

    public async Task<WorkshopStaff?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE Email = @Email";
        return await connection.QuerySingleOrDefaultAsync<WorkshopStaff>(sql, new { Email = email });
    }

    public async Task<IEnumerable<WorkshopStaff>> GetByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM WorkshopStaff 
            WHERE WorkshopOwnerId = @WorkshopOwnerId 
            ORDER BY Name";
        return await connection.QueryAsync<WorkshopStaff>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<IEnumerable<WorkshopStaff>> GetActiveByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM WorkshopStaff 
            WHERE WorkshopOwnerId = @WorkshopOwnerId AND IsActive = 1
            ORDER BY Name";
        return await connection.QueryAsync<WorkshopStaff>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<IEnumerable<WorkshopStaff>> GetInactiveByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM WorkshopStaff 
            WHERE WorkshopOwnerId = @WorkshopOwnerId AND IsActive = 0
            ORDER BY Name";
        return await connection.QueryAsync<WorkshopStaff>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<WorkshopStaff?> UpdateAsync(WorkshopStaff staff)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                Name = @Name,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                Address = @Address,
                PhotoUrl = @PhotoUrl,
                City = @City,
                Role = @Role,
                JobCategories = @JobCategories,
                CanApproveVehicles = @CanApproveVehicles,
                CanApproveInquiries = @CanApproveInquiries,
                CanGenerateEstimates = @CanGenerateEstimates,
                CanCreateJobCard = @CanCreateJobCard,
                CanApproveDisputes = @CanApproveDisputes,
                CanApproveQuotesPayments = @CanApproveQuotesPayments,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE Id = @Id";

        staff.UpdatedAt = DateTime.UtcNow;
        return await connection.QuerySingleOrDefaultAsync<WorkshopStaff>(sql, staff);
    }

    public async Task<bool> UpdatePermissionsAsync(int id, bool canApproveVehicles, bool canApproveInquiries,
        bool canGenerateEstimates, bool canCreateJobCard, bool canApproveDisputes, bool canApproveQuotesPayments)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                CanApproveVehicles = @CanApproveVehicles,
                CanApproveInquiries = @CanApproveInquiries,
                CanGenerateEstimates = @CanGenerateEstimates,
                CanCreateJobCard = @CanCreateJobCard,
                CanApproveDisputes = @CanApproveDisputes,
                CanApproveQuotesPayments = @CanApproveQuotesPayments,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            CanApproveVehicles = canApproveVehicles,
            CanApproveInquiries = canApproveInquiries,
            CanGenerateEstimates = canGenerateEstimates,
            CanCreateJobCard = canCreateJobCard,
            CanApproveDisputes = canApproveDisputes,
            CanApproveQuotesPayments = canApproveQuotesPayments,
            UpdatedAt = DateTime.UtcNow
        });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePhotoAsync(int id, string photoUrl)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                PhotoUrl = @PhotoUrl,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            PhotoUrl = photoUrl,
            UpdatedAt = DateTime.UtcNow
        });
        return rowsAffected > 0;
    }

    public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            IsActive = isActive,
            UpdatedAt = DateTime.UtcNow
        });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM WorkshopStaff WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM WorkshopStaff WHERE WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<int> GetActiveCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM WorkshopStaff WHERE WorkshopOwnerId = @WorkshopOwnerId AND IsActive = 1";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }
}
