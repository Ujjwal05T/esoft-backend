namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a part request inquiry made during a vehicle visit
/// </summary>
public class Inquiry
{
    public int Id { get; set; }
    
    // Foreign Keys
    public int VehicleId { get; set; }
    public int? VehicleVisitId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public int? RequestedByStaffId { get; set; }
    
    // Inquiry Details
    public string InquiryNumber { get; set; } = string.Empty;
    public string JobCategory { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; } = InquiryStatus.Open;
    
    // Dates
    public DateTime PlacedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public DateTime? DeclinedDate { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Inquiry status enum
/// </summary>
public enum InquiryStatus
{
    Open = 0,
    Closed = 1,
    Approved = 2,
    Requested = 3,
    Declined = 4
}
