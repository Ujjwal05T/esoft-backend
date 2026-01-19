using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for workshop registration and onboarding
/// </summary>
public interface IWorkshopRegistrationService
{
    // ==================== Frontend (Mobile App) - Workshop Owner Registration ====================
    
    /// <summary>
    /// Send OTP for phone verification during registration
    /// </summary>
    Task<OtpSentResponse> SendRegistrationOtpAsync(string phoneNumber);
    
    /// <summary>
    /// Verify OTP during registration
    /// </summary>
    Task<OtpVerifiedResponse> VerifyRegistrationOtpAsync(string phoneNumber, string otp);
    
    /// <summary>
    /// Submit workshop registration request
    /// </summary>
    Task<RegistrationSubmittedResponse> SubmitRegistrationAsync(WorkshopRegistrationRequest request);
    
    // ==================== Frontend-Portal (Admin) - Workshop Management ====================
    
    /// <summary>
    /// Get all pending workshop requests for admin panel
    /// </summary>
    Task<WorkshopRequestsResponse> GetPendingRequestsAsync(int page, int pageSize, string? source = null);
    
    /// <summary>
    /// Get all onboarded (active) workshops
    /// </summary>
    Task<List<WorkshopDto>> GetOnboardedWorkshopsAsync();
    
    /// <summary>
    /// Get workshop by ID
    /// </summary>
    Task<WorkshopOwner?> GetWorkshopByIdAsync(int id);
    
    /// <summary>
    /// Send OTP to ETNA team verifier
    /// </summary>
    Task<OtpSentResponse> SendEtnaVerificationOtpAsync(int workshopId, string etnaMobileNumber);
    
    /// <summary>
    /// Verify ETNA team OTP
    /// </summary>
    Task<OtpVerifiedResponse> VerifyEtnaOtpAsync(int workshopId, string etnaMobileNumber, string otp);
    
    /// <summary>
    /// Upload documents for workshop
    /// </summary>
    Task<ApiResponse> UploadDocumentsAsync(int workshopId, string? tradeLicenseUrl, string? workshopPhotoUrl, string? ownerPhotoUrl);
    
    /// <summary>
    /// Complete workshop onboarding
    /// </summary>
    Task<OnboardingCompletedResponse> CompleteOnboardingAsync(int workshopId, CompleteOnboardingRequest request);
    
    /// <summary>
    /// Reject workshop request
    /// </summary>
    Task<ApiResponse> RejectWorkshopAsync(int workshopId, string reason);
}
