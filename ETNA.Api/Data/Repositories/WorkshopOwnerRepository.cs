using System.Data;
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

    public async Task<int> CreateAsync(WorkshopOwner workshopOwner)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO WorkshopOwners (
                OwnerName, PhoneNumber, Email, AadhaarNumber, PasswordHash, OwnerPhotoUrl,
                WorkshopName, Address, Landmark, PinCode, City, 
                TradeLicenseDocumentUrl, WorkshopPhotoUrl,
                ContactPersonName, ContactPersonMobile, GSTNumber,
                Source, ETNAVerifierName, ETNAVerifierPhone,
                IsPhoneVerified, IsETNAVerified, RegistrationStatus,
                CreatedAt, UpdatedAt, ETNAVerifiedAt, ActivatedAt
            )
            OUTPUT INSERTED.Id
            VALUES (
                @OwnerName, @PhoneNumber, @Email, @AadhaarNumber, @PasswordHash, @OwnerPhotoUrl,
                @WorkshopName, @Address, @Landmark, @PinCode, @City,
                @TradeLicenseDocumentUrl, @WorkshopPhotoUrl,
                @ContactPersonName, @ContactPersonMobile, @GSTNumber,
                @Source, @ETNAVerifierName, @ETNAVerifierPhone,
                @IsPhoneVerified, @IsETNAVerified, @RegistrationStatus,
                @CreatedAt, @UpdatedAt, @ETNAVerifiedAt, @ActivatedAt
            )";

        return await connection.ExecuteScalarAsync<int>(sql, workshopOwner);
    }

    public async Task<WorkshopOwner?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM WorkshopOwners WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<WorkshopOwner>(sql, new { Id = id });
    }

    public async Task<WorkshopOwner?> GetByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM WorkshopOwners WHERE PhoneNumber = @PhoneNumber";
        return await connection.QueryFirstOrDefaultAsync<WorkshopOwner>(sql, new { PhoneNumber = phoneNumber });
    }

    public async Task<WorkshopOwner?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM WorkshopOwners WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<WorkshopOwner>(sql, new { Email = email });
    }

    public async Task<List<WorkshopOwner>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM WorkshopOwners ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<WorkshopOwner>(sql);
        return result.ToList();
    }

    public async Task<List<WorkshopOwner>> GetByStatusAsync(RegistrationStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT * FROM WorkshopOwners WHERE RegistrationStatus = @Status ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<WorkshopOwner>(sql, new { Status = (int)status });
        return result.ToList();
    }

    public async Task<List<WorkshopOwner>> GetPendingRequestsAsync(int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT * FROM WorkshopOwners 
            WHERE RegistrationStatus IN (0, 1) 
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        var result = await connection.QueryAsync<WorkshopOwner>(sql, new { Offset = (page - 1) * pageSize, PageSize = pageSize });
        return result.ToList();
    }

    public async Task<int> GetCountByStatusAsync(RegistrationStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT COUNT(*) FROM WorkshopOwners WHERE RegistrationStatus = @Status";
        return await connection.ExecuteScalarAsync<int>(sql, new { Status = (int)status });
    }

    public async Task<int> GetCountBySourceAsync(string source)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT COUNT(*) FROM WorkshopOwners WHERE Source = @Source AND RegistrationStatus IN (0, 1)";
        return await connection.ExecuteScalarAsync<int>(sql, new { Source = source });
    }

    public async Task<bool> UpdateAsync(WorkshopOwner workshopOwner)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET
                OwnerName = @OwnerName,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                AadhaarNumber = @AadhaarNumber,
                PasswordHash = @PasswordHash,
                OwnerPhotoUrl = @OwnerPhotoUrl,
                WorkshopName = @WorkshopName,
                Address = @Address,
                Landmark = @Landmark,
                PinCode = @PinCode,
                City = @City,
                TradeLicenseDocumentUrl = @TradeLicenseDocumentUrl,
                WorkshopPhotoUrl = @WorkshopPhotoUrl,
                ContactPersonName = @ContactPersonName,
                ContactPersonMobile = @ContactPersonMobile,
                GSTNumber = @GSTNumber,
                Source = @Source,
                ETNAVerifierName = @ETNAVerifierName,
                ETNAVerifierPhone = @ETNAVerifierPhone,
                IsPhoneVerified = @IsPhoneVerified,
                IsETNAVerified = @IsETNAVerified,
                RegistrationStatus = @RegistrationStatus,
                UpdatedAt = @UpdatedAt,
                ETNAVerifiedAt = @ETNAVerifiedAt,
                ActivatedAt = @ActivatedAt
            WHERE Id = @Id";

        workshopOwner.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, workshopOwner);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStatusAsync(int id, RegistrationStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET 
                RegistrationStatus = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Status = (int)status, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePhoneVerifiedAsync(int id, bool isVerified)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET 
                IsPhoneVerified = @IsVerified,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IsVerified = isVerified, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateETNAVerificationAsync(int id, string verifierName, string verifierPhone)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET 
                ETNAVerifierName = @VerifierName,
                ETNAVerifierPhone = @VerifierPhone,
                IsETNAVerified = 1,
                ETNAVerifiedAt = @VerifiedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var now = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, VerifierName = verifierName, VerifierPhone = verifierPhone, VerifiedAt = now, UpdatedAt = now });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateDocumentsAsync(int id, string? tradeLicenseUrl, string? workshopPhotoUrl, string? ownerPhotoUrl)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET 
                TradeLicenseDocumentUrl = COALESCE(@TradeLicenseUrl, TradeLicenseDocumentUrl),
                WorkshopPhotoUrl = COALESCE(@WorkshopPhotoUrl, WorkshopPhotoUrl),
                OwnerPhotoUrl = COALESCE(@OwnerPhotoUrl, OwnerPhotoUrl),
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TradeLicenseUrl = tradeLicenseUrl, WorkshopPhotoUrl = workshopPhotoUrl, OwnerPhotoUrl = ownerPhotoUrl, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> ActivateWorkshopAsync(int id, string passwordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE WorkshopOwners SET 
                PasswordHash = @PasswordHash,
                RegistrationStatus = @Status,
                ActivatedAt = @ActivatedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var now = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, PasswordHash = passwordHash, Status = (int)RegistrationStatus.Active, ActivatedAt = now, UpdatedAt = now });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT COUNT(1) FROM WorkshopOwners WHERE PhoneNumber = @PhoneNumber";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { PhoneNumber = phoneNumber });
        return count > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT COUNT(1) FROM WorkshopOwners WHERE Email = @Email";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }
}
