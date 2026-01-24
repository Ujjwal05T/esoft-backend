namespace ETNA.Api.Models.DTOs;

/// <summary>
/// Request DTO for creating a new staff member (by owner)
/// </summary>
public class CreateStaffRequest
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = "staff";
    public string[]? JobCategories { get; set; }
    
    // Permissions
    public bool CanApproveVehicles { get; set; } = false;
    public bool CanApproveInquiries { get; set; } = false;
    public bool CanGenerateEstimates { get; set; } = false;
    public bool CanCreateJobCard { get; set; } = false;
    public bool CanApproveDisputes { get; set; } = false;
    public bool CanApproveQuotesPayments { get; set; } = false;
}

/// <summary>
/// Request DTO for updating staff member details
/// </summary>
public class UpdateStaffRequest
{
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Role { get; set; }
    public string[]? JobCategories { get; set; }
    
    // Permissions
    public bool? CanApproveVehicles { get; set; }
    public bool? CanApproveInquiries { get; set; }
    public bool? CanGenerateEstimates { get; set; }
    public bool? CanCreateJobCard { get; set; }
    public bool? CanApproveDisputes { get; set; }
    public bool? CanApproveQuotesPayments { get; set; }
}

/// <summary>
/// Request DTO for updating staff permissions only
/// </summary>
public class UpdateStaffPermissionsRequest
{
    public bool CanApproveVehicles { get; set; }
    public bool CanApproveInquiries { get; set; }
    public bool CanGenerateEstimates { get; set; }
    public bool CanCreateJobCard { get; set; }
    public bool CanApproveDisputes { get; set; }
    public bool CanApproveQuotesPayments { get; set; }
}

/// <summary>
/// Response DTO for staff member
/// </summary>
public class StaffResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhotoUrl { get; set; }
    public int WorkshopId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public string? City { get; set; }
    public string Role { get; set; } = string.Empty;
    public string[]? JobCategories { get; set; }
    
    // Permissions
    public StaffPermissionsResponse Permissions { get; set; } = new();
    
    // Status
    public bool IsActive { get; set; }
    public bool IsPhoneVerified { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Permissions sub-object for staff response
/// </summary>
public class StaffPermissionsResponse
{
    public bool VehicleApprovals { get; set; }
    public bool InquiryApprovals { get; set; }
    public bool GenerateEstimates { get; set; }
    public bool CreateJobCard { get; set; }
    public bool DisputeApprovals { get; set; }
    public bool QuoteApprovalsPayments { get; set; }
}

/// <summary>
/// Response DTO for staff list
/// </summary>
public class StaffListResponse
{
    public List<StaffResponse> Staff { get; set; } = new();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}
