namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a geographic area/zone entity
/// </summary>
public class Area
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Cities { get; set; } = "[]"; // JSON array: ["Mumbai", "Thane"]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
