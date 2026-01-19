using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for WorkshopOwner operations
/// </summary>
public interface IWorkshopOwnerRepository
{
    // Create
    Task<int> CreateAsync(WorkshopOwner workshopOwner);
    
    // Read
    Task<WorkshopOwner?> GetByIdAsync(int id);
    Task<WorkshopOwner?> GetByPhoneNumberAsync(string phoneNumber);
    Task<WorkshopOwner?> GetByEmailAsync(string email);
    Task<List<WorkshopOwner>> GetAllAsync();
    Task<List<WorkshopOwner>> GetByStatusAsync(RegistrationStatus status);
    Task<List<WorkshopOwner>> GetPendingRequestsAsync(int page, int pageSize);
    Task<int> GetCountByStatusAsync(RegistrationStatus status);
    Task<int> GetCountBySourceAsync(string source);
    
    // Update
    Task<bool> UpdateAsync(WorkshopOwner workshopOwner);
    Task<bool> UpdateStatusAsync(int id, RegistrationStatus status);
    Task<bool> UpdatePhoneVerifiedAsync(int id, bool isVerified);
    Task<bool> UpdateETNAVerificationAsync(int id, string verifierName, string verifierPhone);
    Task<bool> UpdateDocumentsAsync(int id, string? tradeLicenseUrl, string? workshopPhotoUrl, string? ownerPhotoUrl);
    Task<bool> ActivateWorkshopAsync(int id, string passwordHash);
    
    // Check existence
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    Task<bool> ExistsByEmailAsync(string email);
}
