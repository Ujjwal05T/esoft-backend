using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for WorkshopStaff using Dapper
/// </summary>
public class WorkshopStaffRepository : IWorkshopStaffRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public WorkshopStaffRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<WorkshopStaff?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<WorkshopStaff>(sql, new { Id = id });
    }
    
    public async Task<WorkshopStaff?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<WorkshopStaff>(sql, new { Email = email });
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM WorkshopStaff WHERE Email = @Email";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }
    
    public async Task<WorkshopStaff?> GetByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE PhoneNumber = @PhoneNumber";
        return await connection.QueryFirstOrDefaultAsync<WorkshopStaff>(sql, new { PhoneNumber = phoneNumber });
    }
    
    public async Task<int> CreateAsync(WorkshopStaff staff)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO WorkshopStaff 
            (Name, Email, PhoneNumber, City, PasswordHash, WorkshopOwnerId, 
             IsPhoneVerified, IsActive, RegistrationStatus, CreatedAt)
            VALUES 
            (@Name, @Email, @PhoneNumber, @City, @PasswordHash, @WorkshopOwnerId,
             @IsPhoneVerified, @IsActive, @RegistrationStatus, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";
        
        return await connection.ExecuteScalarAsync<int>(sql, staff);
    }
    
    public async Task<bool> UpdateAsync(WorkshopStaff staff)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                Name = @Name,
                PhoneNumber = @PhoneNumber,
                City = @City,
                PhotoUrl = @PhotoUrl,
                WorkshopOwnerId = @WorkshopOwnerId,
                IsPhoneVerified = @IsPhoneVerified,
                IsActive = @IsActive,
                RegistrationStatus = @RegistrationStatus,
                UpdatedAt = @UpdatedAt,
                ApprovedAt = @ApprovedAt,
                ApprovedByOwnerId = @ApprovedByOwnerId,
                RejectionReason = @RejectionReason
            WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, staff);
        return rowsAffected > 0;
    }
    
    public async Task<IEnumerable<WorkshopStaff>> GetByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopStaff WHERE WorkshopOwnerId = @WorkshopOwnerId ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<WorkshopStaff>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }
    
    public async Task<IEnumerable<WorkshopStaff>> GetPendingApprovalsByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM WorkshopStaff 
            WHERE WorkshopOwnerId = @WorkshopOwnerId 
              AND RegistrationStatus = @Status 
            ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<WorkshopStaff>(sql, new 
        { 
            WorkshopOwnerId = workshopOwnerId,
            Status = StaffRegistrationStatus.PendingOwnerApproval
        });
    }
    
    public async Task<IEnumerable<WorkshopStaff>> GetActiveStaffByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM WorkshopStaff 
            WHERE WorkshopOwnerId = @WorkshopOwnerId 
              AND IsActive = 1 
              AND RegistrationStatus = @Status 
            ORDER BY Name";
        return await connection.QueryAsync<WorkshopStaff>(sql, new 
        { 
            WorkshopOwnerId = workshopOwnerId,
            Status = StaffRegistrationStatus.Approved
        });
    }
    
    public async Task<bool> UpdatePhoneVerificationAsync(int id, bool isVerified, StaffRegistrationStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                IsPhoneVerified = @IsVerified,
                RegistrationStatus = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            IsVerified = isVerified, 
            Status = status,
            UpdatedAt = DateTime.UtcNow 
        });
        return rowsAffected > 0;
    }
    
    public async Task<bool> UpdateApprovalStatusAsync(int id, StaffRegistrationStatus status, int? approvedByOwnerId, string? rejectionReason)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopStaff SET
                RegistrationStatus = @Status,
                IsActive = @IsActive,
                ApprovedAt = @ApprovedAt,
                ApprovedByOwnerId = @ApprovedByOwnerId,
                RejectionReason = @RejectionReason,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        var isActive = status == StaffRegistrationStatus.Approved;
        var approvedAt = isActive ? DateTime.UtcNow : (DateTime?)null;
        
        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            Status = status,
            IsActive = isActive,
            ApprovedAt = approvedAt,
            ApprovedByOwnerId = approvedByOwnerId,
            RejectionReason = rejectionReason,
            UpdatedAt = DateTime.UtcNow 
        });
        return rowsAffected > 0;
    }
}
