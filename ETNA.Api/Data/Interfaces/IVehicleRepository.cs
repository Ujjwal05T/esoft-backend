using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

public interface IVehicleRepository
{
    Task<int> CreateAsync(Vehicle vehicle);
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle?> GetByPlateNumberAsync(string plateNumber, int workshopOwnerId);
    Task<List<Vehicle>> GetByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<List<Vehicle>> GetByStatusAsync(int workshopOwnerId, VehicleStatus status);
    Task<bool> UpdateAsync(Vehicle vehicle);
    Task<bool> UpdateStatusAsync(int id, VehicleStatus status);
    Task<bool> DeleteAsync(int id);
    Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId);
}
