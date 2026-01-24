using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VehicleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Vehicle vehicle)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO Vehicles (
                PlateNumber, Brand, Model, Year, Variant, ChassisNumber, Specs,
                RegistrationName, OwnerName, ContactNumber, Email, GstNumber, InsuranceProvider,
                OdometerReading, Observations, ObservationsAudioUrl,
                WorkshopOwnerId, Status, CreatedAt, UpdatedAt
            )
            OUTPUT INSERTED.Id
            VALUES (
                @PlateNumber, @Brand, @Model, @Year, @Variant, @ChassisNumber, @Specs,
                @RegistrationName, @OwnerName, @ContactNumber, @Email, @GstNumber, @InsuranceProvider,
                @OdometerReading, @Observations, @ObservationsAudioUrl,
                @WorkshopOwnerId, @Status, @CreatedAt, @UpdatedAt
            )";

        return await connection.ExecuteScalarAsync<int>(sql, vehicle);
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Vehicles WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Vehicle>(sql, new { Id = id });
    }

    public async Task<Vehicle?> GetByPlateNumberAsync(string plateNumber, int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Vehicles WHERE PlateNumber = @PlateNumber AND WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.QueryFirstOrDefaultAsync<Vehicle>(sql, new { PlateNumber = plateNumber, WorkshopOwnerId = workshopOwnerId });
    }

    public async Task<List<Vehicle>> GetByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Vehicles WHERE WorkshopOwnerId = @WorkshopOwnerId ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Vehicle>(sql, new { WorkshopOwnerId = workshopOwnerId });
        return result.ToList();
    }

    public async Task<List<Vehicle>> GetByStatusAsync(int workshopOwnerId, VehicleStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Vehicles WHERE WorkshopOwnerId = @WorkshopOwnerId AND Status = @Status ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Vehicle>(sql, new { WorkshopOwnerId = workshopOwnerId, Status = (int)status });
        return result.ToList();
    }

    public async Task<bool> UpdateAsync(Vehicle vehicle)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE Vehicles SET
                PlateNumber = @PlateNumber,
                Brand = @Brand,
                Model = @Model,
                Year = @Year,
                Variant = @Variant,
                ChassisNumber = @ChassisNumber,
                Specs = @Specs,
                RegistrationName = @RegistrationName,
                OwnerName = @OwnerName,
                ContactNumber = @ContactNumber,
                Email = @Email,
                GstNumber = @GstNumber,
                InsuranceProvider = @InsuranceProvider,
                OdometerReading = @OdometerReading,
                Observations = @Observations,
                ObservationsAudioUrl = @ObservationsAudioUrl,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        vehicle.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, vehicle);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStatusAsync(int id, VehicleStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Vehicles SET Status = @Status, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Status = (int)status, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Vehicles WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Vehicles WHERE WorkshopOwnerId = @WorkshopOwnerId";
        return await connection.ExecuteScalarAsync<int>(sql, new { WorkshopOwnerId = workshopOwnerId });
    }
}
