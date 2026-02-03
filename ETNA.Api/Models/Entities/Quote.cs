namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a quote created by a sales person in response to an inquiry
/// </summary>
public class Quote
{
    public int Id { get; set; }

    // Quote Number
    public string QuoteNumber { get; set; } = string.Empty;

    // Foreign Keys
    public int InquiryId { get; set; }
    public int VehicleId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public int? CreatedByStaffId { get; set; }

    // Charges
    public decimal PackingCharges { get; set; } = 0m;
    public decimal ForwardingCharges { get; set; } = 0m;
    public decimal ShippingCharges { get; set; } = 0m;
    public decimal TotalAmount { get; set; } = 0m;

    // TODO: Convert Status to enum (pending, approved, rejected) — keeping as string for now
    public string Status { get; set; } = "pending";

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
