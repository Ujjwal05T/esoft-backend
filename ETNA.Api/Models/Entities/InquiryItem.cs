namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents an individual part item within an inquiry
/// </summary>
public class InquiryItem
{
    public int Id { get; set; }
    
    // Foreign Key
    public int InquiryId { get; set; }
    
    // Part Details
    public string PartName { get; set; } = string.Empty;
    public string PreferredBrand { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Remark { get; set; } = string.Empty;
    
    // Media
    public string? AudioUrl { get; set; }
    public int? AudioDuration { get; set; }
    public string? Image1Url { get; set; }
    public string? Image2Url { get; set; }
    public string? Image3Url { get; set; }
    
    // Timestamp
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
