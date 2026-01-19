namespace ETNA.Api.Models.DTOs;

// ==================== REQUEST DTOs ====================

/// <summary>
/// Request to send OTP for phone verification
/// </summary>
public record SendOtpRequest(string PhoneNumber);

/// <summary>
/// Request to verify OTP
/// </summary>
public record VerifyOtpRequest(string PhoneNumber, string Otp);

/// <summary>
/// Workshop registration request from mobile app
/// </summary>
public record WorkshopRegistrationRequest(
    string OwnerName,
    string PhoneNumber,
    string? Email,
    string AadhaarNumber,
    string WorkshopName,
    string Address,
    string? Landmark,
    string PinCode,
    string City,
    string? ContactPersonName,
    string? ContactPersonMobile,
    string? GSTNumber
);

/// <summary>
/// Request to send OTP to ETNA team verifier
/// </summary>
public record EtnaSendOtpRequest(
    int WorkshopId,
    string EtnaTeamMobileNumber
);

/// <summary>
/// Request to verify ETNA team OTP
/// </summary>
public record EtnaVerifyOtpRequest(
    int WorkshopId,
    string EtnaTeamMobileNumber,
    string Otp
);

/// <summary>
/// Request to complete workshop onboarding
/// </summary>
public record CompleteOnboardingRequest(
    string? EtnaVerifierName,
    bool GeneratePassword = true,
    bool SendCredentials = true
);

// ==================== RESPONSE DTOs ====================

/// <summary>
/// Generic API response
/// </summary>
public record ApiResponse(
    bool Success,
    string Message
);

/// <summary>
/// API response with data
/// </summary>
public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data
) : ApiResponse(Success, Message);

/// <summary>
/// OTP send response
/// </summary>
public record OtpSentResponse(
    bool Success,
    string Message,
    int ExpiresInSeconds
);

/// <summary>
/// OTP verification response
/// </summary>
public record OtpVerifiedResponse(
    bool Success,
    string Message,
    string? Token
);

/// <summary>
/// Workshop registration submitted response
/// </summary>
public record RegistrationSubmittedResponse(
    bool Success,
    string Message,
    int WorkshopId,
    string Status
);

/// <summary>
/// Workshop request DTO for admin panel
/// </summary>
public record WorkshopRequestDto(
    int Id,
    string WorkshopName,
    string OwnerName,
    string PhoneNumber,
    string? Email,
    string AadhaarNumber,
    string Address,
    string? Landmark,
    string PinCode,
    string City,
    string? ContactPersonName,
    string? ContactPersonMobile,
    string? GSTNumber,
    string Source,
    string Status,
    bool IsPhoneVerified,
    bool IsETNAVerified,
    DateTime CreatedAt,
    int DaysAgo
);

/// <summary>
/// Workshop DTO for onboarded workshops list
/// </summary>
public record WorkshopDto(
    int Id,
    string WorkshopName,
    string OwnerName,
    string? Email,
    string PhoneNumber,
    string City,
    string Status,
    DateTime CreatedAt,
    DateTime? ActivatedAt
);

/// <summary>
/// Workshop request stats
/// </summary>
public record WorkshopRequestStats(
    int PendingOnboarding,
    int FromWhatsApp,
    int FromPhoneCalls,
    int FromApp
);

/// <summary>
/// Paginated workshop requests response
/// </summary>
public record WorkshopRequestsResponse(
    List<WorkshopRequestDto> Requests,
    WorkshopRequestStats Stats,
    int TotalCount,
    int Page,
    int PageSize
);

/// <summary>
/// Onboarding completed response
/// </summary>
public record OnboardingCompletedResponse(
    bool Success,
    string Message,
    int WorkshopId,
    string WorkshopName,
    string? OwnerEmail,
    bool CredentialsSent
);
