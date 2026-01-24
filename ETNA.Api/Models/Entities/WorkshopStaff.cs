namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a workshop staff member entity
/// </summary>
public class WorkshopStaff
{
    public int Id { get; set; }
    
    // Staff Details
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? Address { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Workshop Reference
    public int WorkshopId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public string? City { get; set; }
    
    // Role & Categories
    public string Role { get; set; } = "staff"; // 'staff', 'manager', 'technician', etc.
    public string? JobCategories { get; set; } // JSON array: ["Engine", "Brake System", "AC"]
    
    // Permissions (6 individual boolean columns)
    public bool CanApproveVehicles { get; set; } = false;
    public bool CanApproveInquiries { get; set; } = false;
    public bool CanGenerateEstimates { get; set; } = false;
    public bool CanCreateJobCard { get; set; } = false;
    public bool CanApproveDisputes { get; set; } = false;
    public bool CanApproveQuotesPayments { get; set; } = false;
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsPhoneVerified { get; set; } = false;
    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Active;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
