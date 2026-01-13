using System.ComponentModel.DataAnnotations;

namespace ETNA.Api.Models.DTOs;

/// <summary>
/// DTO for Step 1: Staff basic info
/// </summary>
public class StaffBasicInfoDto
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Step 2: Workshop selection
/// </summary>
public class StaffWorkshopSelectionDto
{
    [Required(ErrorMessage = "Workshop is required")]
    public int WorkshopOwnerId { get; set; }
}

/// <summary>
/// DTO for Step 3: Contact details and registration
/// </summary>
public class StaffRegisterDto
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Workshop is required")]
    public int WorkshopOwnerId { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for phone verification
/// </summary>
public class StaffVerifyPhoneDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = string.Empty;
}

/// <summary>
/// DTO for owner approval/rejection
/// </summary>
public class StaffApprovalDto
{
    [Required(ErrorMessage = "Staff ID is required")]
    public int StaffId { get; set; }
    
    public bool Approve { get; set; }
    
    public string? RejectionReason { get; set; }
}

/// <summary>
/// DTO for staff response data
/// </summary>
public class StaffResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int WorkshopOwnerId { get; set; }
    public string? WorkshopName { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsActive { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// DTO for workshop list (for dropdown)
/// </summary>
public class WorkshopListItemDto
{
    public int Id { get; set; }
    public string WorkshopName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// DTO for pending staff requests (for owner)
/// </summary>
public class PendingStaffRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
