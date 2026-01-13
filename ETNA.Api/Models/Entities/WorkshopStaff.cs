namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a staff member who works at a workshop.
/// Staff must be approved by the workshop owner to gain access.
/// </summary>
public class WorkshopStaff
{
    public int Id { get; set; }
    
    /// <summary>
    /// Full name of the staff member
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address (used for OTP verification and login)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Contact phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// City where the staff member is located
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// URL/path to the staff member's photo
    /// </summary>
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// Foreign key to the workshop owner
    /// </summary>
    public int WorkshopOwnerId { get; set; }
    
    /// <summary>
    /// Whether the staff's phone has been verified via SMS OTP
    /// </summary>
    public bool IsPhoneVerified { get; set; } = false;
    
    /// <summary>
    /// Whether the staff account is active
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Current status in the registration/approval flow
    /// </summary>
    public StaffRegistrationStatus RegistrationStatus { get; set; } = StaffRegistrationStatus.PendingPhoneVerification;
    
    /// <summary>
    /// Date when the staff member registered
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the staff record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Date when the staff was approved by owner
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// ID of the owner who approved/rejected the staff
    /// </summary>
    public int? ApprovedByOwnerId { get; set; }
    
    /// <summary>
    /// Reason for rejection (if rejected by owner)
    /// </summary>
    public string? RejectionReason { get; set; }
    
    // Navigation property (for convenience, not used by Dapper directly)
    public WorkshopOwner? WorkshopOwner { get; set; }
}

/// <summary>
/// Status of staff registration and approval process
/// </summary>
public enum StaffRegistrationStatus
{
    /// <summary>
    /// Staff has registered but phone not yet verified
    /// </summary>
    PendingPhoneVerification = 0,
    
    /// <summary>
    /// Phone verified, waiting for workshop owner approval
    /// </summary>
    PendingOwnerApproval = 1,
    
    /// <summary>
    /// Owner has approved, account is active
    /// </summary>
    Approved = 2,
    
    /// <summary>
    /// Owner has rejected the request
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Account suspended by owner
    /// </summary>
    Suspended = 4
}
