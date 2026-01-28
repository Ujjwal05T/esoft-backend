using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehicleController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(
        IVehicleRepository vehicleRepository,
        IFileUploadService fileUploadService,
        ILogger<VehicleController> logger)
    {
        _vehicleRepository = vehicleRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new vehicle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> CreateVehicle([FromBody] CreateVehicleRequest request)
    {
        // For now, use a default workshop owner ID (in production, get from JWT)
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        var vehicle = new Vehicle
        {
            PlateNumber = request.PlateNumber,
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year,
            Variant = request.Variant,
            ChassisNumber = request.ChassisNumber,
            Specs = request.Specs,
            RegistrationName = request.RegistrationName,
            OwnerName = request.OwnerName,
            ContactNumber = request.ContactNumber,
            Email = request.Email,
            GstNumber = request.GstNumber,
            InsuranceProvider = request.InsuranceProvider,
            OdometerReading = request.OdometerReading,
            Observations = request.Observations,
            WorkshopOwnerId = workshopOwnerId,
            Status = VehicleStatus.Inactive,  // New vehicles start as not in workshop
            CreatedAt = DateTime.UtcNow
        };

        var id = await _vehicleRepository.CreateAsync(vehicle);
        vehicle.Id = id;

        _logger.LogInformation("Vehicle created: {PlateNumber} for workshop {WorkshopId}", request.PlateNumber, workshopOwnerId);

        return CreatedAtAction(nameof(GetVehicle), new { id }, MapToResponse(vehicle));
    }

    /// <summary>
    /// Create vehicle with audio file
    /// </summary>
    [HttpPost("with-audio")]
    public async Task<ActionResult<VehicleResponse>> CreateVehicleWithAudio([FromForm] CreateVehicleRequest request, IFormFile? audioFile)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        string? audioUrl = null;
        if (audioFile != null && audioFile.Length > 0)
        {
            using var stream = audioFile.OpenReadStream();
            audioUrl = await _fileUploadService.UploadFileAsync(stream, audioFile.FileName, $"vehicles/{workshopOwnerId}");
        }

        var vehicle = new Vehicle
        {
            PlateNumber = request.PlateNumber,
            Brand = request.Brand,
            Model = request.Model,
            Year = request.Year,
            Variant = request.Variant,
            ChassisNumber = request.ChassisNumber,
            Specs = request.Specs,
            RegistrationName = request.RegistrationName,
            OwnerName = request.OwnerName,
            ContactNumber = request.ContactNumber,
            Email = request.Email,
            GstNumber = request.GstNumber,
            InsuranceProvider = request.InsuranceProvider,
            OdometerReading = request.OdometerReading,
            Observations = request.Observations,
            ObservationsAudioUrl = audioUrl,
            WorkshopOwnerId = workshopOwnerId,
            Status = VehicleStatus.Inactive,  // New vehicles start as not in workshop
            CreatedAt = DateTime.UtcNow
        };

        var id = await _vehicleRepository.CreateAsync(vehicle);
        vehicle.Id = id;

        _logger.LogInformation("Vehicle created with audio: {PlateNumber}", request.PlateNumber);

        return CreatedAtAction(nameof(GetVehicle), new { id }, MapToResponse(vehicle));
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleResponse>> GetVehicle(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        return Ok(MapToResponse(vehicle));
    }

    /// <summary>
    /// Get all vehicles for the workshop
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<VehicleListResponse>> GetVehicles()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var vehicles = await _vehicleRepository.GetByWorkshopOwnerIdAsync(workshopOwnerId);
        var count = await _vehicleRepository.GetCountByWorkshopOwnerIdAsync(workshopOwnerId);

        return Ok(new VehicleListResponse(
            vehicles.Select(MapToResponse).ToList(),
            count
        ));
    }

    /// <summary>
    /// Get vehicles by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<VehicleListResponse>> GetVehiclesByStatus(int status)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var vehicles = await _vehicleRepository.GetByStatusAsync(workshopOwnerId, (VehicleStatus)status);

        return Ok(new VehicleListResponse(
            vehicles.Select(MapToResponse).ToList(),
            vehicles.Count
        ));
    }

    /// <summary>
    /// Update a vehicle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleResponse>> UpdateVehicle(int id, [FromBody] UpdateVehicleRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        vehicle.PlateNumber = request.PlateNumber;
        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.Variant = request.Variant;
        vehicle.ChassisNumber = request.ChassisNumber;
        vehicle.Specs = request.Specs;
        vehicle.RegistrationName = request.RegistrationName;
        vehicle.OwnerName = request.OwnerName;
        vehicle.ContactNumber = request.ContactNumber;
        vehicle.Email = request.Email;
        vehicle.GstNumber = request.GstNumber;
        vehicle.InsuranceProvider = request.InsuranceProvider;
        vehicle.OdometerReading = request.OdometerReading;
        vehicle.Observations = request.Observations;
        vehicle.Status = (VehicleStatus)request.Status;

        await _vehicleRepository.UpdateAsync(vehicle);

        _logger.LogInformation("Vehicle updated: {Id}", id);

        return Ok(MapToResponse(vehicle));
    }

    /// <summary>
    /// Update vehicle status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateVehicleStatus(int id, [FromBody] int status)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        await _vehicleRepository.UpdateStatusAsync(id, (VehicleStatus)status);

        _logger.LogInformation("Vehicle status updated: {Id} -> {Status}", id, status);

        return Ok(new { message = "Status updated successfully" });
    }

    /// <summary>
    /// Delete a vehicle
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        await _vehicleRepository.DeleteAsync(id);

        _logger.LogInformation("Vehicle deleted: {Id}", id);

        return Ok(new { message = "Vehicle deleted successfully" });
    }

    private int? GetWorkshopOwnerIdFromToken()
    {
        var claim = User.FindFirst("WorkshopOwnerId");
        if (claim != null && int.TryParse(claim.Value, out var id))
        {
            return id;
        }
        return null;
    }

    private static VehicleResponse MapToResponse(Vehicle vehicle)
    {
        return new VehicleResponse(
            vehicle.Id,
            vehicle.PlateNumber,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Variant,
            vehicle.ChassisNumber,
            vehicle.Specs,
            vehicle.RegistrationName,
            vehicle.OwnerName,
            vehicle.ContactNumber,
            vehicle.Email,
            vehicle.GstNumber,
            vehicle.InsuranceProvider,
            vehicle.OdometerReading,
            vehicle.Observations,
            vehicle.ObservationsAudioUrl,
            vehicle.WorkshopOwnerId,
            vehicle.Status.ToString(),
            vehicle.CreatedAt,
            vehicle.UpdatedAt
        );
    }
}
