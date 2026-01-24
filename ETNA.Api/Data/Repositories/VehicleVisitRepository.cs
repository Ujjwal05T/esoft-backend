using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

public class VehicleVisitRepository : IVehicleVisitRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VehicleVisitRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(VehicleVisit visit)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO VehicleVisits (
                VehicleId, WorkshopOwnerId, Status,
                GateInDateTime, GateInDriverName, GateInDriverContact,
                GateInOdometerReading, GateInFuelLevel, GateInProblemShared,
                GateInProblemAudioUrl, GateInImages,
                GateOutDateTime, GateOutDriverName, GateOutDriverContact,
                GateOutOdometerReading, GateOutFuelLevel, GateOutImages,
                CreatedAt, UpdatedAt
            )
            OUTPUT INSERTED.Id
            VALUES (
                @VehicleId, @WorkshopOwnerId, @Status,
                @GateInDateTime, @GateInDriverName, @GateInDriverContact,
                @GateInOdometerReading, @GateInFuelLevel, @GateInProblemShared,
                @GateInProblemAudioUrl, @GateInImages,
                @GateOutDateTime, @GateOutDriverName, @GateOutDriverContact,
                @GateOutOdometerReading, @GateOutFuelLevel, @GateOutImages,
                @CreatedAt, @UpdatedAt
            )";

        return await connection.ExecuteScalarAsync<int>(sql, visit);
    }

    public async Task<VehicleVisit?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM VehicleVisits WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<VehicleVisit>(sql, new { Id = id });
    }

    public async Task<bool> UpdateAsync(VehicleVisit visit)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE VehicleVisits SET
                Status = @Status,
                GateInDriverName = @GateInDriverName,
                GateInDriverContact = @GateInDriverContact,
                GateInOdometerReading = @GateInOdometerReading,
                GateInFuelLevel = @GateInFuelLevel,
                GateInProblemShared = @GateInProblemShared,
                GateInProblemAudioUrl = @GateInProblemAudioUrl,
                GateInImages = @GateInImages,
                GateOutDateTime = @GateOutDateTime,
                GateOutDriverName = @GateOutDriverName,
                GateOutDriverContact = @GateOutDriverContact,
                GateOutOdometerReading = @GateOutOdometerReading,
                GateOutFuelLevel = @GateOutFuelLevel,
                GateOutImages = @GateOutImages,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        visit.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, visit);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM VehicleVisits WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<VehicleVisit>> GetByVehicleIdAsync(int vehicleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM VehicleVisits WHERE VehicleId = @VehicleId ORDER BY GateInDateTime DESC";
        var result = await connection.QueryAsync<VehicleVisit>(sql, new { VehicleId = vehicleId });
        return result.ToList();
    }

    public async Task<List<VehicleVisit>> GetByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM VehicleVisits WHERE WorkshopOwnerId = @WorkshopOwnerId ORDER BY GateInDateTime DESC";
        var result = await connection.QueryAsync<VehicleVisit>(sql, new { WorkshopOwnerId = workshopOwnerId });
        return result.ToList();
    }

    public async Task<List<VehicleVisit>> GetByStatusAsync(int workshopOwnerId, VehicleVisitStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM VehicleVisits 
            WHERE WorkshopOwnerId = @WorkshopOwnerId AND Status = @Status 
            ORDER BY GateInDateTime DESC";
        var result = await connection.QueryAsync<VehicleVisit>(sql, new { WorkshopOwnerId = workshopOwnerId, Status = (int)status });
        return result.ToList();
    }

    public async Task<VehicleVisit?> GetActiveVisitByVehicleIdAsync(int vehicleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM VehicleVisits 
            WHERE VehicleId = @VehicleId AND Status = @Status 
            ORDER BY GateInDateTime DESC";
        return await connection.QueryFirstOrDefaultAsync<VehicleVisit>(sql, new { VehicleId = vehicleId, Status = (int)VehicleVisitStatus.In });
    }

    public async Task<VehicleVisit?> GetLatestVisitByVehicleIdAsync(int vehicleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 * FROM VehicleVisits 
            WHERE VehicleId = @VehicleId 
            ORDER BY GateInDateTime DESC";
        return await connection.QueryFirstOrDefaultAsync<VehicleVisit>(sql, new { VehicleId = vehicleId });
    }

    public async Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM VehicleVisits WHERE WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<int> GetActiveCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM VehicleVisits WHERE WorkshopOwnerId = @WorkshopOwnerId AND Status = @Status";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId, Status = (int)VehicleVisitStatus.In });
    }

    public async Task<bool> GateOutAsync(int visitId, string driverName, string driverContact, DateTime gateOutDateTime, string? odometerReading, int? fuelLevel, string? images)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE VehicleVisits SET
                Status = @Status,
                GateOutDateTime = @GateOutDateTime,
                GateOutDriverName = @GateOutDriverName,
                GateOutDriverContact = @GateOutDriverContact,
                GateOutOdometerReading = @GateOutOdometerReading,
                GateOutFuelLevel = @GateOutFuelLevel,
                GateOutImages = @GateOutImages,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = visitId,
            Status = (int)VehicleVisitStatus.Out,
            GateOutDateTime = gateOutDateTime,
            GateOutDriverName = driverName,
            GateOutDriverContact = driverContact,
            GateOutOdometerReading = odometerReading,
            GateOutFuelLevel = fuelLevel,
            GateOutImages = images,
            UpdatedAt = DateTime.UtcNow
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateGateInImagesAsync(int visitId, string images)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE VehicleVisits SET GateInImages = @Images, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = visitId, Images = images, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateGateOutImagesAsync(int visitId, string images)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE VehicleVisits SET GateOutImages = @Images, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = visitId, Images = images, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }
}
