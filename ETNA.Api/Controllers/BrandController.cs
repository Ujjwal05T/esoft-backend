using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Controllers;

/// <summary>
/// API Controller for Brand management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<BrandController> _logger;

    public BrandController(IBrandRepository brandRepository, ILogger<BrandController> logger)
    {
        _brandRepository = brandRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all brands
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandResponse>>> GetAll()
    {
        try
        {
            var brands = await _brandRepository.GetAllAsync();
            var response = brands.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all brands");
            return StatusCode(500, new { error = "An error occurred while fetching brands" });
        }
    }

    /// <summary>
    /// Get active brands only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BrandResponse>>> GetActive()
    {
        try
        {
            var brands = await _brandRepository.GetActiveAsync();
            var response = brands.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active brands");
            return StatusCode(500, new { error = "An error occurred while fetching brands" });
        }
    }

    /// <summary>
    /// Get brand by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BrandResponse>> GetById(int id)
    {
        try
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound(new { error = "Brand not found" });
            }
            return Ok(MapToResponse(brand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the brand" });
        }
    }

    /// <summary>
    /// Create a new brand
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BrandResponse>> Create([FromBody] CreateBrandRequest request)
    {
        try
        {
            // Check if brand name already exists
            var existing = await _brandRepository.GetByNameAsync(request.Name);
            if (existing != null)
            {
                return BadRequest(new { error = "A brand with this name already exists" });
            }

            var brand = new Brand
            {
                Name = request.Name,
                LogoUrl = request.LogoUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _brandRepository.CreateAsync(brand);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating brand");
            return StatusCode(500, new { error = "An error occurred while creating the brand" });
        }
    }

    /// <summary>
    /// Update an existing brand
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BrandResponse>> Update(int id, [FromBody] UpdateBrandRequest request)
    {
        try
        {
            var existing = await _brandRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Brand not found" });
            }

            // Check if new name conflicts with another brand
            var nameConflict = await _brandRepository.GetByNameAsync(request.Name);
            if (nameConflict != null && nameConflict.Id != id)
            {
                return BadRequest(new { error = "A brand with this name already exists" });
            }

            existing.Name = request.Name;
            existing.LogoUrl = request.LogoUrl;
            existing.IsActive = request.IsActive;

            var updated = await _brandRepository.UpdateAsync(existing);
            return Ok(MapToResponse(updated!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating brand {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the brand" });
        }
    }

    /// <summary>
    /// Delete a brand
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _brandRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Brand not found" });
            }

            await _brandRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting brand {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the brand" });
        }
    }

    private static BrandResponse MapToResponse(Brand brand)
    {
        return new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            IsActive = brand.IsActive,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}
