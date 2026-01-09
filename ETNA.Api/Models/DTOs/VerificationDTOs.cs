using System.ComponentModel.DataAnnotations;

namespace ETNA.Api.Models.DTOs;

/// <summary>
/// DTO for ETNA team verification request
/// </summary>
public class ETNAVerificationDto
{
    [Required(ErrorMessage = "Owner email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string OwnerEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ETNA team member name is required")]
    public string ETNAMemberName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ETNA team member phone is required")]
    public string ETNAMemberPhone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ETNA OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "ETNA OTP must be 6 digits")]
    public string ETNAOtp { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Owner OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Owner OTP must be 6 digits")]
    public string OwnerOtp { get; set; } = string.Empty;
}

/// <summary>
/// DTO for initiating ETNA verification (sends OTP to owner)
/// </summary>
public class InitiateETNAVerificationDto
{
    [Required(ErrorMessage = "Owner email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string OwnerEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ETNA team member name is required")]
    public string ETNAMemberName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ETNA team member phone is required")]
    public string ETNAMemberPhone { get; set; } = string.Empty;
}

/// <summary>
/// DTO for photo upload request
/// </summary>
public class PhotoUploadDto
{
    [Required(ErrorMessage = "Owner email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string OwnerEmail { get; set; } = string.Empty;
}

/// <summary>
/// DTO for ETNA verification response
/// </summary>
public class ETNAVerificationResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? NextStep { get; set; }
    public string? RegistrationStatus { get; set; }
}

/// <summary>
/// DTO for photo upload response
/// </summary>
public class PhotoUploadResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? OwnerPhotoUrl { get; set; }
    public string? WorkshopPhotoUrl { get; set; }
    public string? RegistrationStatus { get; set; }
    public bool IsActive { get; set; }
}
