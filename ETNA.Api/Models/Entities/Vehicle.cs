namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a vehicle registered in the workshop
/// </summary>
public class Vehicle
{
    public int Id { get; set; }
    
    // Vehicle Details
    public string PlateNumber { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Variant { get; set; }
    public string? ChassisNumber { get; set; }
    public string? Specs { get; set; }  // e.g., "2.4L ZX MT/Diesel"
    
    // Registration Details
    public string? RegistrationName { get; set; }  // Name on RC
    
    // Owner Details
    public string OwnerName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? GstNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    
    // Service Details (legacy - for backward compatibility)
    public string? OdometerReading { get; set; }
    public string? Observations { get; set; }
    public string? ObservationsAudioUrl { get; set; }
    
    // Workshop Association
    public int WorkshopOwnerId { get; set; }
    
    // Status
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum VehicleStatus
{
    Active = 0,
    InService = 1,
    Completed = 2,
    Archived = 3
}
