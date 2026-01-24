using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

public interface IVehicleVisitRepository
{
    // CRUD Operations
    Task<int> CreateAsync(VehicleVisit visit);
    Task<VehicleVisit?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(VehicleVisit visit);
    Task<bool> DeleteAsync(int id);
    
    // Query Operations
    Task<List<VehicleVisit>> GetByVehicleIdAsync(int vehicleId);
    Task<List<VehicleVisit>> GetByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<List<VehicleVisit>> GetByStatusAsync(int workshopOwnerId, VehicleVisitStatus status);
    Task<VehicleVisit?> GetActiveVisitByVehicleIdAsync(int vehicleId);
    Task<VehicleVisit?> GetLatestVisitByVehicleIdAsync(int vehicleId);
    
    // Count Operations
    Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<int> GetActiveCountByWorkshopOwnerIdAsync(int workshopOwnerId);
    
    // Gate Out Operation
    Task<bool> GateOutAsync(int visitId, string driverName, string driverContact, DateTime gateOutDateTime, string? odometerReading, int? fuelLevel, string? images);
    
    // Update Images
    Task<bool> UpdateGateInImagesAsync(int visitId, string images);
    Task<bool> UpdateGateOutImagesAsync(int visitId, string images);
}
