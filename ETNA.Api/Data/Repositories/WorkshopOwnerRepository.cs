using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for WorkshopOwner using Dapper
/// </summary>
public class WorkshopOwnerRepository : IWorkshopOwnerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public WorkshopOwnerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<WorkshopOwner?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopOwners WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<WorkshopOwner>(sql, new { Id = id });
    }
    
    public async Task<WorkshopOwner?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopOwners WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<WorkshopOwner>(sql, new { Email = email });
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM WorkshopOwners WHERE Email = @Email";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }
    
    public async Task<int> CreateAsync(WorkshopOwner owner)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO WorkshopOwners 
            (OwnerName, Email, PhoneNumber, PasswordHash, WorkshopName, Address, City, TradeLicense, 
             IsEmailVerified, IsETNAVerified, IsActive, RegistrationStatus, CreatedAt)
            VALUES 
            (@OwnerName, @Email, @PhoneNumber, @PasswordHash, @WorkshopName, @Address, @City, @TradeLicense,
             @IsEmailVerified, @IsETNAVerified, @IsActive, @RegistrationStatus, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";
        
        return await connection.ExecuteScalarAsync<int>(sql, owner);
    }
    
    public async Task<bool> UpdateAsync(WorkshopOwner owner)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopOwners SET
                OwnerName = @OwnerName,
                PhoneNumber = @PhoneNumber,
                WorkshopName = @WorkshopName,
                Address = @Address,
                City = @City,
                TradeLicense = @TradeLicense,
                TradeLicenseDocumentUrl = @TradeLicenseDocumentUrl,
                OwnerPhotoUrl = @OwnerPhotoUrl,
                WorkshopPhotoUrl = @WorkshopPhotoUrl,
                ETNAVerifierName = @ETNAVerifierName,
                ETNAVerifierPhone = @ETNAVerifierPhone,
                IsEmailVerified = @IsEmailVerified,
                IsETNAVerified = @IsETNAVerified,
                IsActive = @IsActive,
                RegistrationStatus = @RegistrationStatus,
                UpdatedAt = @UpdatedAt,
                ETNAVerifiedAt = @ETNAVerifiedAt,
                ActivatedAt = @ActivatedAt
            WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, owner);
        return rowsAffected > 0;
    }
    
    public async Task<bool> UpdateEmailVerificationAsync(int id, bool isVerified, WorkshopOwnerRegistrationStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE WorkshopOwners SET
                IsEmailVerified = @IsVerified,
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
    
    public async Task<IEnumerable<WorkshopOwner>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopOwners ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<WorkshopOwner>(sql);
    }
    
    public async Task<IEnumerable<WorkshopOwner>> GetByCityAsync(string city)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopOwners WHERE City = @City AND IsActive = 1 ORDER BY WorkshopName";
        return await connection.QueryAsync<WorkshopOwner>(sql, new { City = city });
    }
    
    public async Task<IEnumerable<WorkshopOwner>> GetActiveWorkshopsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM WorkshopOwners WHERE IsActive = 1 ORDER BY WorkshopName";
        return await connection.QueryAsync<WorkshopOwner>(sql);
    }
}
