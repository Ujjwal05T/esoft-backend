namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a vehicle visit (gate in/out) at a workshop
/// Tracks the entire lifecycle of a vehicle's service visit
/// </summary>
public class VehicleVisit
{
    public int Id { get; set; }
    
    // Foreign Keys
    public int VehicleId { get; set; }
    public int WorkshopOwnerId { get; set; }
    
    // Visit Status
    public VehicleVisitStatus Status { get; set; } = VehicleVisitStatus.In;
    
    // ========== GATE IN DETAILS ==========
    public DateTime GateInDateTime { get; set; }
    public string GateInDriverName { get; set; } = string.Empty;
    public string GateInDriverContact { get; set; } = string.Empty;
    public string? GateInOdometerReading { get; set; }
    public int? GateInFuelLevel { get; set; }  // 0-100 percentage
    public string? GateInProblemShared { get; set; }
    public string? GateInProblemAudioUrl { get; set; }
    public string? GateInImages { get; set; }  // JSON array of image URLs
    
    // ========== GATE OUT DETAILS ==========
    public DateTime? GateOutDateTime { get; set; }
    public string? GateOutDriverName { get; set; }
    public string? GateOutDriverContact { get; set; }
    public string? GateOutOdometerReading { get; set; }
    public int? GateOutFuelLevel { get; set; }  // 0-100 percentage
    public string? GateOutImages { get; set; }  // JSON array of image URLs
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property (for reference, not used with Dapper)
    // public Vehicle? Vehicle { get; set; }
}

/// <summary>
/// Vehicle visit status enum
/// </summary>
public enum VehicleVisitStatus
{
    In = 0,      // Vehicle is currently in the workshop
    Out = 1      // Vehicle has been released (gate out completed)
}
