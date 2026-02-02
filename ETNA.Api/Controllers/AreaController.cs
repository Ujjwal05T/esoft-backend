using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Controllers;

/// <summary>
/// API Controller for Area management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AreaController : ControllerBase
{
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<AreaController> _logger;

    public AreaController(IAreaRepository areaRepository, ILogger<AreaController> logger)
    {
        _areaRepository = areaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all areas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AreaResponse>>> GetAll()
    {
        try
        {
            var areas = await _areaRepository.GetAllAsync();
            var response = areas.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all areas");
            return StatusCode(500, new { error = "An error occurred while fetching areas" });
        }
    }

    /// <summary>
    /// Get area by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AreaResponse>> GetById(int id)
    {
        try
        {
            var area = await _areaRepository.GetByIdAsync(id);
            if (area == null)
            {
                return NotFound(new { error = "Area not found" });
            }
            return Ok(MapToResponse(area));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting area {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the area" });
        }
    }

    /// <summary>
    /// Get areas by state
    /// </summary>
    [HttpGet("by-state/{state}")]
    public async Task<ActionResult<IEnumerable<AreaResponse>>> GetByState(string state)
    {
        try
        {
            var areas = await _areaRepository.GetByStateAsync(state);
            var response = areas.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting areas for state {State}", state);
            return StatusCode(500, new { error = "An error occurred while fetching areas" });
        }
    }

    /// <summary>
    /// Create a new area
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AreaResponse>> Create([FromBody] CreateAreaRequest request)
    {
        try
        {
            var area = new Area
            {
                Name = request.Name,
                State = request.State,
                Cities = JsonSerializer.Serialize(request.Cities),
                CreatedAt = DateTime.UtcNow
            };

            var created = await _areaRepository.CreateAsync(area);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating area");
            return StatusCode(500, new { error = "An error occurred while creating the area" });
        }
    }

    /// <summary>
    /// Update an existing area
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AreaResponse>> Update(int id, [FromBody] UpdateAreaRequest request)
    {
        try
        {
            var existing = await _areaRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Area not found" });
            }

            existing.Name = request.Name;
            existing.State = request.State;
            existing.Cities = JsonSerializer.Serialize(request.Cities);

            var updated = await _areaRepository.UpdateAsync(existing);
            return Ok(MapToResponse(updated!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating area {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the area" });
        }
    }

    /// <summary>
    /// Delete an area
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _areaRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Area not found" });
            }

            await _areaRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting area {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the area" });
        }
    }

    private static AreaResponse MapToResponse(Area area)
    {
        List<string> cities;
        try
        {
            cities = JsonSerializer.Deserialize<List<string>>(area.Cities) ?? new List<string>();
        }
        catch
        {
            cities = new List<string>();
        }

        return new AreaResponse
        {
            Id = area.Id,
            Name = area.Name,
            State = area.State,
            Cities = cities,
            CreatedAt = area.CreatedAt,
            UpdatedAt = area.UpdatedAt
        };
    }
}
