namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a workshop owner and their workshop.
/// Each owner has exactly one workshop - they are registered together.
/// </summary>
public class WorkshopOwner
{
    public int Id { get; set; }
    
    // ===== Owner Details =====
    
    /// <summary>
    /// Full name of the workshop owner
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Owner's email address (used for OTP verification and login)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Owner's contact phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// URL/path to the owner's photo (collected during verification)
    /// </summary>
    public string? OwnerPhotoUrl { get; set; }
    
    // ===== Workshop Details =====
    
    /// <summary>
    /// Name of the workshop
    /// </summary>
    public string WorkshopName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full address of the workshop
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// City where the workshop is located (used for staff filtering)
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Trade license number for the workshop
    /// </summary>
    public string TradeLicense { get; set; } = string.Empty;
    
    /// <summary>
    /// URL/path to the trade license document
    /// </summary>
    public string? TradeLicenseDocumentUrl { get; set; }
    
    /// <summary>
    /// URL/path to the workshop photo (collected during verification)
    /// </summary>
    public string? WorkshopPhotoUrl { get; set; }
    
    // ===== ETNA Verification Details =====
    
    /// <summary>
    /// Name of the ETNA team member who verified
    /// </summary>
    public string? ETNAVerifierName { get; set; }
    
    /// <summary>
    /// Phone number of the ETNA team member
    /// </summary>
    public string? ETNAVerifierPhone { get; set; }
    
    // ===== Verification & Status =====
    
    /// <summary>
    /// Whether the owner's email has been verified via OTP
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;
    
    /// <summary>
    /// Whether ETNA team verification is complete
    /// </summary>
    public bool IsETNAVerified { get; set; } = false;
    
    /// <summary>
    /// Whether the workshop is verified and active
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Current status in the registration/verification flow
    /// </summary>
    public WorkshopOwnerRegistrationStatus RegistrationStatus { get; set; } = WorkshopOwnerRegistrationStatus.PendingEmailVerification;
    
    // ===== Timestamps =====
    
    /// <summary>
    /// Date when the workshop was registered
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Date when ETNA verification was completed
    /// </summary>
    public DateTime? ETNAVerifiedAt { get; set; }
    
    /// <summary>
    /// Date when photos were uploaded and account became active
    /// </summary>
    public DateTime? ActivatedAt { get; set; }
}

/// <summary>
/// Status of workshop owner registration and verification process
/// </summary>
public enum WorkshopOwnerRegistrationStatus
{
    /// <summary>
    /// Owner has registered but email not yet verified
    /// </summary>
    PendingEmailVerification = 0,
    
    /// <summary>
    /// Email verified, waiting for ETNA team physical visit
    /// </summary>
    PendingETNAVerification = 1,
    
    /// <summary>
    /// ETNA verification complete, pending photo uploads
    /// </summary>
    PendingPhotoUpload = 2,
    
    /// <summary>
    /// All verifications complete, workshop is active
    /// </summary>
    Active = 3,
    
    /// <summary>
    /// Registration was rejected
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Account suspended
    /// </summary>
    Suspended = 5
}
