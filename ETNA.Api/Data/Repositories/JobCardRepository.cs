using Dapper;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;
using Microsoft.Data.SqlClient;

namespace ETNA.Api.Data.Repositories;

public class JobCardRepository : IJobCardRepository
{
    private readonly string _connectionString;

    public JobCardRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<int> CreateAsync(JobCard jobCard)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            INSERT INTO JobCards (
                VehicleId, VehicleVisitId, WorkshopOwnerId, JobCategory,
                AssignedStaffId, AssignedStaffName, Remark, AudioUrl, Images, Videos,
                Status, Priority, EstimatedCost, ActualCost, EstimatedDuration,
                CreatedAt
            )
            VALUES (
                @VehicleId, @VehicleVisitId, @WorkshopOwnerId, @JobCategory,
                @AssignedStaffId, @AssignedStaffName, @Remark, @AudioUrl, @Images, @Videos,
                @Status, @Priority, @EstimatedCost, @ActualCost, @EstimatedDuration,
                @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.ExecuteScalarAsync<int>(sql, jobCard);
    }

    public async Task<JobCard?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "SELECT * FROM JobCards WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<JobCard>(sql, new { Id = id });
    }

    public async Task<List<JobCard>> GetByVehicleIdAsync(int vehicleId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT * FROM JobCards 
            WHERE VehicleId = @VehicleId 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<JobCard>(sql, new { VehicleId = vehicleId });
        return result.ToList();
    }

    public async Task<List<JobCard>> GetByVehicleVisitIdAsync(int vehicleVisitId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT * FROM JobCards 
            WHERE VehicleVisitId = @VehicleVisitId 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<JobCard>(sql, new { VehicleVisitId = vehicleVisitId });
        return result.ToList();
    }

    public async Task<List<JobCard>> GetByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT * FROM JobCards 
            WHERE WorkshopOwnerId = @WorkshopOwnerId 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<JobCard>(sql, new { WorkshopOwnerId = workshopOwnerId });
        return result.ToList();
    }

    public async Task<List<JobCard>> GetByAssignedStaffIdAsync(int staffId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT * FROM JobCards 
            WHERE AssignedStaffId = @StaffId 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<JobCard>(sql, new { StaffId = staffId });
        return result.ToList();
    }

    public async Task<List<JobCard>> GetByStatusAsync(int workshopOwnerId, JobCardStatus status)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            SELECT * FROM JobCards 
            WHERE WorkshopOwnerId = @WorkshopOwnerId 
            AND Status = @Status 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<JobCard>(sql, new { WorkshopOwnerId = workshopOwnerId, Status = status.ToString() });
        return result.ToList();
    }

    public async Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "SELECT COUNT(*) FROM JobCards WHERE WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<bool> UpdateAsync(JobCard jobCard)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE JobCards SET
                JobCategory = @JobCategory,
                AssignedStaffId = @AssignedStaffId,
                AssignedStaffName = @AssignedStaffName,
                Remark = @Remark,
                AudioUrl = @AudioUrl,
                Images = @Images,
                Videos = @Videos,
                Status = @Status,
                Priority = @Priority,
                EstimatedCost = @EstimatedCost,
                ActualCost = @ActualCost,
                EstimatedDuration = @EstimatedDuration,
                StartedAt = @StartedAt,
                CompletedAt = @CompletedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, jobCard);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStatusAsync(int id, JobCardStatus status)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE JobCards SET
                Status = @Status,
                UpdatedAt = @UpdatedAt,
                StartedAt = CASE WHEN @Status = 'InProgress' AND StartedAt IS NULL THEN @UpdatedAt ELSE StartedAt END,
                CompletedAt = CASE WHEN @Status = 'Completed' THEN @UpdatedAt ELSE CompletedAt END
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            Status = status.ToString(), 
            UpdatedAt = DateTime.UtcNow 
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> AssignStaffAsync(int id, int staffId, string staffName)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = @"
            UPDATE JobCards SET
                AssignedStaffId = @StaffId,
                AssignedStaffName = @StaffName,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            StaffId = staffId, 
            StaffName = staffName,
            UpdatedAt = DateTime.UtcNow 
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var sql = "DELETE FROM JobCards WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
