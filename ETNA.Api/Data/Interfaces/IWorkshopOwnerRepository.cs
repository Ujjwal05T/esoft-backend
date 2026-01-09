using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for WorkshopOwner data operations
/// </summary>
public interface IWorkshopOwnerRepository
{
    Task<WorkshopOwner?> GetByIdAsync(int id);
    Task<WorkshopOwner?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<int> CreateAsync(WorkshopOwner owner);
    Task<bool> UpdateAsync(WorkshopOwner owner);
    Task<bool> UpdateEmailVerificationAsync(int id, bool isVerified, WorkshopOwnerRegistrationStatus status);
    Task<IEnumerable<WorkshopOwner>> GetAllAsync();
    Task<IEnumerable<WorkshopOwner>> GetByCityAsync(string city);
    Task<IEnumerable<WorkshopOwner>> GetActiveWorkshopsAsync();
}
