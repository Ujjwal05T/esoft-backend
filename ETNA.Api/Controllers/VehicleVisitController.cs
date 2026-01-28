using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehicleVisitController : ControllerBase
{
    private readonly IVehicleVisitRepository _visitRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<VehicleVisitController> _logger;

    public VehicleVisitController(
        IVehicleVisitRepository visitRepository,
        IVehicleRepository vehicleRepository,
        IFileUploadService fileUploadService,
        ILogger<VehicleVisitController> logger)
    {
        _visitRepository = visitRepository;
        _vehicleRepository = vehicleRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new vehicle visit (Gate In)
    /// </summary>
    [HttpPost("gate-in")]
    public async Task<ActionResult<VehicleVisitResponse>> GateIn([FromBody] CreateVehicleVisitRequest request)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        // Verify vehicle exists
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Check if vehicle already has an active visit
        var activeVisit = await _visitRepository.GetActiveVisitByVehicleIdAsync(request.VehicleId);
        if (activeVisit != null)
        {
            return BadRequest(new { message = "Vehicle already has an active visit. Please gate out first." });
        }

        var visit = new VehicleVisit
        {
            VehicleId = request.VehicleId,
            WorkshopOwnerId = workshopOwnerId,
            Status = VehicleVisitStatus.In,
            GateInDateTime = request.GateInDateTime,
            GateInDriverName = request.GateInDriverName,
            GateInDriverContact = request.GateInDriverContact,
            GateInOdometerReading = request.GateInOdometerReading,
            GateInFuelLevel = request.GateInFuelLevel,
            GateInProblemShared = request.GateInProblemShared,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _visitRepository.CreateAsync(visit);
        visit.Id = id;

        // Update vehicle status to Active (in workshop)
        await _vehicleRepository.UpdateStatusAsync(request.VehicleId, VehicleStatus.Active);

        _logger.LogInformation("Gate In completed for vehicle {VehicleId}, visit {VisitId}", request.VehicleId, id);

        return CreatedAtAction(nameof(GetVisit), new { id }, await MapToResponseAsync(visit));
    }

    /// <summary>
    /// Gate In with audio and images
    /// </summary>
    [HttpPost("gate-in/with-media")]
    public async Task<ActionResult<VehicleVisitResponse>> GateInWithMedia(
        [FromForm] CreateVehicleVisitRequest request,
        IFormFile? audioFile,
        List<IFormFile>? images)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        // Verify vehicle exists
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Check if vehicle already has an active visit
        var activeVisit = await _visitRepository.GetActiveVisitByVehicleIdAsync(request.VehicleId);
        if (activeVisit != null)
        {
            return BadRequest(new { message = "Vehicle already has an active visit. Please gate out first." });
        }

        // Upload audio file
        string? audioUrl = null;
        if (audioFile != null && audioFile.Length > 0)
        {
            using var stream = audioFile.OpenReadStream();
            audioUrl = await _fileUploadService.UploadFileAsync(stream, audioFile.FileName, $"visits/{workshopOwnerId}/audio");
        }

        // Upload images
        var imageUrls = new List<string>();
        if (images != null)
        {
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    using var stream = image.OpenReadStream();
                    var url = await _fileUploadService.UploadFileAsync(stream, image.FileName, $"visits/{workshopOwnerId}/images");
                    imageUrls.Add(url);
                }
            }
        }

        var visit = new VehicleVisit
        {
            VehicleId = request.VehicleId,
            WorkshopOwnerId = workshopOwnerId,
            Status = VehicleVisitStatus.In,
            GateInDateTime = request.GateInDateTime,
            GateInDriverName = request.GateInDriverName,
            GateInDriverContact = request.GateInDriverContact,
            GateInOdometerReading = request.GateInOdometerReading,
            GateInFuelLevel = request.GateInFuelLevel,
            GateInProblemShared = request.GateInProblemShared,
            GateInProblemAudioUrl = audioUrl,
            GateInImages = imageUrls.Count > 0 ? JsonSerializer.Serialize(imageUrls) : null,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _visitRepository.CreateAsync(visit);
        visit.Id = id;

        // Update vehicle status to Active (in workshop)
        await _vehicleRepository.UpdateStatusAsync(request.VehicleId, VehicleStatus.Active);

        _logger.LogInformation("Gate In with media completed for vehicle {VehicleId}, visit {VisitId}", request.VehicleId, id);

        return CreatedAtAction(nameof(GetVisit), new { id }, await MapToResponseAsync(visit));
    }

    /// <summary>
    /// Complete gate out for a vehicle visit
    /// </summary>
    [HttpPost("{id}/gate-out")]
    public async Task<ActionResult<VehicleVisitResponse>> GateOut(int id, [FromBody] GateOutRequest request)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null)
        {
            return NotFound(new { message = "Visit not found" });
        }

        if (visit.Status == VehicleVisitStatus.Out)
        {
            return BadRequest(new { message = "Vehicle has already been gated out" });
        }

        var gateOutDateTime = request.GateOutDateTime ?? DateTime.UtcNow;
        
        var success = await _visitRepository.GateOutAsync(
            id,
            request.GateOutDriverName,
            request.GateOutDriverContact,
            gateOutDateTime,
            request.GateOutOdometerReading,
            request.GateOutFuelLevel,
            null
        );

        if (!success)
        {
            return StatusCode(500, new { message = "Failed to complete gate out" });
        }

        // Update vehicle status to Inactive (not in workshop)
        await _vehicleRepository.UpdateStatusAsync(visit.VehicleId, VehicleStatus.Inactive);

        _logger.LogInformation("Gate Out completed for visit {VisitId}", id);

        // Fetch updated visit
        var updatedVisit = await _visitRepository.GetByIdAsync(id);
        return Ok(await MapToResponseAsync(updatedVisit!));
    }

    /// <summary>
    /// Get visit by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleVisitResponse>> GetVisit(int id)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null)
        {
            return NotFound(new { message = "Visit not found" });
        }

        return Ok(await MapToResponseAsync(visit));
    }

    /// <summary>
    /// Get all visits for the workshop
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<VehicleVisitListResponse>> GetVisits()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var visits = await _visitRepository.GetByWorkshopOwnerIdAsync(workshopOwnerId);
        var count = await _visitRepository.GetCountByWorkshopOwnerIdAsync(workshopOwnerId);

        var responses = new List<VehicleVisitResponse>();
        foreach (var visit in visits)
        {
            responses.Add(await MapToResponseAsync(visit));
        }

        return Ok(new VehicleVisitListResponse(responses, count));
    }

    /// <summary>
    /// Get vehicles currently in workshop (active visits)
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<VehicleVisitListResponse>> GetCurrentVehicles()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var visits = await _visitRepository.GetByStatusAsync(workshopOwnerId, VehicleVisitStatus.In);
        var count = await _visitRepository.GetActiveCountByWorkshopOwnerIdAsync(workshopOwnerId);

        var responses = new List<VehicleVisitResponse>();
        foreach (var visit in visits)
        {
            responses.Add(await MapToResponseAsync(visit));
        }

        return Ok(new VehicleVisitListResponse(responses, count));
    }

    /// <summary>
    /// Get visit history (completed gate outs)
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<VehicleVisitListResponse>> GetVisitHistory()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var visits = await _visitRepository.GetByStatusAsync(workshopOwnerId, VehicleVisitStatus.Out);

        var responses = new List<VehicleVisitResponse>();
        foreach (var visit in visits)
        {
            responses.Add(await MapToResponseAsync(visit));
        }

        return Ok(new VehicleVisitListResponse(responses, responses.Count));
    }

    /// <summary>
    /// Get all visits for a specific vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    public async Task<ActionResult<VehicleVisitListResponse>> GetVehicleVisits(int vehicleId)
    {
        var visits = await _visitRepository.GetByVehicleIdAsync(vehicleId);

        var responses = new List<VehicleVisitResponse>();
        foreach (var visit in visits)
        {
            responses.Add(await MapToResponseAsync(visit));
        }

        return Ok(new VehicleVisitListResponse(responses, responses.Count));
    }

    /// <summary>
    /// Get active visit for a specific vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/active")]
    public async Task<ActionResult<VehicleVisitResponse>> GetActiveVehicleVisit(int vehicleId)
    {
        var visit = await _visitRepository.GetActiveVisitByVehicleIdAsync(vehicleId);
        if (visit == null)
        {
            return NotFound(new { message = "No active visit found for this vehicle" });
        }

        return Ok(await MapToResponseAsync(visit));
    }

    /// <summary>
    /// Delete a visit
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVisit(int id)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null)
        {
            return NotFound(new { message = "Visit not found" });
        }

        await _visitRepository.DeleteAsync(id);

        _logger.LogInformation("Visit deleted: {Id}", id);

        return Ok(new { message = "Visit deleted successfully" });
    }

    /// <summary>
    /// Get workshop summary (vehicles in/out count)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<WorkshopVehicleSummary>> GetWorkshopSummary()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        
        var totalIn = await _visitRepository.GetActiveCountByWorkshopOwnerIdAsync(workshopOwnerId);
        var totalAll = await _visitRepository.GetCountByWorkshopOwnerIdAsync(workshopOwnerId);
        var totalOut = totalAll - totalIn;
        
        var currentVisits = await _visitRepository.GetByStatusAsync(workshopOwnerId, VehicleVisitStatus.In);
        
        var currentResponses = new List<VehicleVisitResponse>();
        foreach (var visit in currentVisits)
        {
            currentResponses.Add(await MapToResponseAsync(visit));
        }

        return Ok(new WorkshopVehicleSummary(totalIn, totalOut, currentResponses));
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

    private async Task<VehicleVisitResponse> MapToResponseAsync(VehicleVisit visit)
    {
        // Get vehicle info
        var vehicle = await _vehicleRepository.GetByIdAsync(visit.VehicleId);
        VehicleBasicInfo? vehicleInfo = null;
        
        if (vehicle != null)
        {
            vehicleInfo = new VehicleBasicInfo(
                vehicle.Id,
                vehicle.PlateNumber,
                vehicle.Brand,
                vehicle.Model,
                vehicle.Year,
                vehicle.Variant,
                vehicle.Specs,
                vehicle.OwnerName,
                vehicle.ContactNumber
            );
        }

        // Parse image JSON arrays
        List<string>? gateInImages = null;
        List<string>? gateOutImages = null;
        
        if (!string.IsNullOrEmpty(visit.GateInImages))
        {
            try
            {
                gateInImages = JsonSerializer.Deserialize<List<string>>(visit.GateInImages);
            }
            catch { }
        }
        
        if (!string.IsNullOrEmpty(visit.GateOutImages))
        {
            try
            {
                gateOutImages = JsonSerializer.Deserialize<List<string>>(visit.GateOutImages);
            }
            catch { }
        }

        return new VehicleVisitResponse(
            visit.Id,
            visit.VehicleId,
            visit.WorkshopOwnerId,
            visit.Status.ToString(),
            visit.GateInDateTime,
            visit.GateInDriverName,
            visit.GateInDriverContact,
            visit.GateInOdometerReading,
            visit.GateInFuelLevel,
            visit.GateInProblemShared,
            visit.GateInProblemAudioUrl,
            gateInImages,
            visit.GateOutDateTime,
            visit.GateOutDriverName,
            visit.GateOutDriverContact,
            visit.GateOutOdometerReading,
            visit.GateOutFuelLevel,
            gateOutImages,
            vehicleInfo,
            visit.CreatedAt,
            visit.UpdatedAt
        );
    }
}
