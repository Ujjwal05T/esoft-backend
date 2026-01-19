namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a workshop owner entity with registration and verification status
/// </summary>
public class WorkshopOwner
{
    public int Id { get; set; }
    
    // Owner Details
    public string OwnerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string AadhaarNumber { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? OwnerPhotoUrl { get; set; }
    
    // Workshop Details
    public string WorkshopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public string PinCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? TradeLicenseDocumentUrl { get; set; }
    public string? WorkshopPhotoUrl { get; set; }
    
    // Contact Person (if different from owner)
    public string? ContactPersonName { get; set; }
    public string? ContactPersonMobile { get; set; }
    
    // Business Registration
    public string? GSTNumber { get; set; }
    
    // Source tracking
    public string Source { get; set; } = "app"; // 'app', 'whatsapp', 'phone_call'
    
    // ETNA Verification Details
    public string? ETNAVerifierName { get; set; }
    public string? ETNAVerifierPhone { get; set; }
    
    // Verification Status
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsETNAVerified { get; set; } = false;
    
    // Registration Status: 0=Pending, 1=UnderReview, 2=Active, 3=Rejected
    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Pending;
    
    // Computed property for JWT
    public bool IsActive => RegistrationStatus == RegistrationStatus.Active;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ETNAVerifiedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
}

public enum RegistrationStatus
{
    Pending = 0,
    UnderReview = 1,
    Active = 2,
    Rejected = 3
}
