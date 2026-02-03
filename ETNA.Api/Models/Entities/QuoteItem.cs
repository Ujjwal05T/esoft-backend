namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents an individual part line item within a quote
/// </summary>
public class QuoteItem
{
    public int Id { get; set; }

    // Foreign Keys
    public int QuoteId { get; set; }
    public int InquiryItemId { get; set; }

    // Part Details
    public string PartName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Mrp { get; set; }
    public decimal UnitPrice { get; set; }

    // Availability: in_stock, out_of_stock, on_order, discontinued
    public string Availability { get; set; } = "in_stock";
    public DateTime? EstimatedDelivery { get; set; }

    // Timestamp
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
