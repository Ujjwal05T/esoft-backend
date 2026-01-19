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
    
    // Workshop Reference
    public int WorkshopId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public string? City { get; set; }
    
    // Role
    public string Role { get; set; } = "staff"; // 'staff', 'manager', etc.
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsPhoneVerified { get; set; } = false;
    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Active;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
