using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

public interface IJobCardRepository
{
    Task<int> CreateAsync(JobCard jobCard);
    Task<JobCard?> GetByIdAsync(int id);
    Task<List<JobCard>> GetByVehicleIdAsync(int vehicleId);
    Task<List<JobCard>> GetByVehicleVisitIdAsync(int vehicleVisitId);
    Task<List<JobCard>> GetByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<List<JobCard>> GetByAssignedStaffIdAsync(int staffId);
    Task<List<JobCard>> GetByStatusAsync(int workshopOwnerId, JobCardStatus status);
    Task<int> GetCountByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<bool> UpdateAsync(JobCard jobCard);
    Task<bool> UpdateStatusAsync(int id, JobCardStatus status);
    Task<bool> AssignStaffAsync(int id, int staffId, string staffName);
    Task<bool> DeleteAsync(int id);
}
