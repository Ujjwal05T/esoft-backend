using System.ComponentModel.DataAnnotations;

namespace ETNA.Api.Models.DTOs;

/// <summary>
/// DTO for creating a new area
/// </summary>
public class CreateAreaRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    public List<string> Cities { get; set; } = new();
}

/// <summary>
/// DTO for updating an area
/// </summary>
public class UpdateAreaRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    public List<string> Cities { get; set; } = new();
}

/// <summary>
/// DTO for area response
/// </summary>
public class AreaResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public List<string> Cities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new brand
/// </summary>
public class CreateBrandRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? LogoUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating a brand
/// </summary>
public class UpdateBrandRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? LogoUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for brand response
/// </summary>
public class BrandResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
