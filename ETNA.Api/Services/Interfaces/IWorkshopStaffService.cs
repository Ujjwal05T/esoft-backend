using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for WorkshopStaff operations
/// </summary>
public interface IWorkshopStaffService
{
    /// <summary>
    /// Register a new staff member
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> RegisterAsync(StaffRegisterDto dto);
    
    /// <summary>
    /// Verify staff phone with SMS OTP
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> VerifyPhoneAsync(StaffVerifyPhoneDto dto);
    
    /// <summary>
    /// Resend OTP to staff phone
    /// </summary>
    Task<ServiceResult<bool>> ResendOtpAsync(string phoneNumber);
    
    /// <summary>
    /// Get staff by ID
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> GetByIdAsync(int id);
    
    /// <summary>
    /// Get staff by email
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> GetByEmailAsync(string email);
    
    /// <summary>
    /// Get workshops by city (for dropdown)
    /// </summary>
    Task<ServiceResult<IEnumerable<WorkshopListItemDto>>> GetWorkshopsByCityAsync(string city);
    
    /// <summary>
    /// Get pending approval requests for a workshop owner
    /// </summary>
    Task<ServiceResult<IEnumerable<PendingStaffRequestDto>>> GetPendingRequestsAsync(int workshopOwnerId);
    
    /// <summary>
    /// Approve or reject a staff request
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> ProcessApprovalAsync(StaffApprovalDto dto, int approverOwnerId);
    
    /// <summary>
    /// Get all staff for a workshop
    /// </summary>
    Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetStaffByWorkshopAsync(int workshopOwnerId);
    
    /// <summary>
    /// Check registration status
    /// </summary>
    Task<ServiceResult<StaffResponseDto>> CheckStatusAsync(string email);
}
