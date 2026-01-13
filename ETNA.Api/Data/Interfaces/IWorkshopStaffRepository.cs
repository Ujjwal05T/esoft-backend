using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for WorkshopStaff operations
/// </summary>
public interface IWorkshopStaffRepository
{
    /// <summary>
    /// Get staff by ID
    /// </summary>
    Task<WorkshopStaff?> GetByIdAsync(int id);
    
    /// <summary>
    /// Get staff by email
    /// </summary>
    Task<WorkshopStaff?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Check if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
    
    /// <summary>
    /// Get staff by phone number
    /// </summary>
    Task<WorkshopStaff?> GetByPhoneNumberAsync(string phoneNumber);
    
    /// <summary>
    /// Create new staff
    /// </summary>
    Task<int> CreateAsync(WorkshopStaff staff);
    
    /// <summary>
    /// Update staff
    /// </summary>
    Task<bool> UpdateAsync(WorkshopStaff staff);
    
    /// <summary>
    /// Get all staff for a workshop
    /// </summary>
    Task<IEnumerable<WorkshopStaff>> GetByWorkshopOwnerIdAsync(int workshopOwnerId);
    
    /// <summary>
    /// Get pending approval requests for a workshop
    /// </summary>
    Task<IEnumerable<WorkshopStaff>> GetPendingApprovalsByWorkshopOwnerIdAsync(int workshopOwnerId);
    
    /// <summary>
    /// Get active staff for a workshop
    /// </summary>
    Task<IEnumerable<WorkshopStaff>> GetActiveStaffByWorkshopOwnerIdAsync(int workshopOwnerId);
    
    /// <summary>
    /// Update phone verification status
    /// </summary>
    Task<bool> UpdatePhoneVerificationAsync(int id, bool isVerified, StaffRegistrationStatus status);
    
    /// <summary>
    /// Update approval status
    /// </summary>
    Task<bool> UpdateApprovalStatusAsync(int id, StaffRegistrationStatus status, int? approvedByOwnerId, string? rejectionReason);
}
