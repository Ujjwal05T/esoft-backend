using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for WorkshopStaff data access
/// </summary>
public interface IWorkshopStaffRepository
{
    // Create
    Task<WorkshopStaff> CreateAsync(WorkshopStaff staff);
    
    // Read
    Task<WorkshopStaff?> GetByIdAsync(int id);
    Task<WorkshopStaff?> GetByPhoneNumberAsync(string phoneNumber);
    Task<WorkshopStaff?> GetByEmailAsync(string email);
    Task<IEnumerable<WorkshopStaff>> GetByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<IEnumerable<WorkshopStaff>> GetActiveByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<IEnumerable<WorkshopStaff>> GetInactiveByWorkshopOwnerIdAsync(int workshopOwnerId);
    
    // Update
    Task<WorkshopStaff?> UpdateAsync(WorkshopStaff staff);
    Task<bool> UpdatePermissionsAsync(int id, bool canApproveVehicles, bool canApproveInquiries, 
        bool canGenerateEstimates, bool canCreateJobCard, bool canApproveDisputes, bool canApproveQuotesPayments);
    Task<bool> UpdatePhotoAsync(int id, string photoUrl);
    Task<bool> SetActiveStatusAsync(int id, bool isActive);
    
    // Delete
    Task<bool> DeleteAsync(int id);
    
    // Counts
    Task<int> GetTotalCountByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<int> GetActiveCountByWorkshopOwnerIdAsync(int workshopOwnerId);
}
