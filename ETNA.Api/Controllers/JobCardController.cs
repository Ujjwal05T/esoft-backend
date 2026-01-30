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
public class JobCardController : ControllerBase
{
    private readonly IJobCardRepository _jobCardRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<JobCardController> _logger;

    public JobCardController(
        IJobCardRepository jobCardRepository,
        IVehicleRepository vehicleRepository,
        IWorkshopStaffRepository staffRepository,
        IFileUploadService fileUploadService,
        ILogger<JobCardController> logger)
    {
        _jobCardRepository = jobCardRepository;
        _vehicleRepository = vehicleRepository;
        _staffRepository = staffRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new job card
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JobCardResponse>> CreateJobCard([FromBody] CreateJobCardRequest request)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        // Verify vehicle exists
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Get staff name if staff is assigned
        string? staffName = null;
        if (request.AssignedStaffId.HasValue)
        {
            var staff = await _staffRepository.GetByIdAsync(request.AssignedStaffId.Value);
            staffName = staff?.Name;
        }

        // Parse priority
        if (!Enum.TryParse<JobPriority>(request.Priority, true, out var priority))
        {
            priority = JobPriority.Normal;
        }

        var jobCard = new JobCard
        {
            VehicleId = request.VehicleId,
            VehicleVisitId = request.VehicleVisitId,
            WorkshopOwnerId = workshopOwnerId,
            JobCategory = request.JobCategory,
            AssignedStaffId = request.AssignedStaffId,
            AssignedStaffName = staffName,
            Remark = request.Remark,
            Status = JobCardStatus.Pending,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _jobCardRepository.CreateAsync(jobCard);
        jobCard.Id = id;

        _logger.LogInformation("Job card created: {Id} for vehicle {VehicleId}", id, request.VehicleId);

        return CreatedAtAction(nameof(GetJobCard), new { id }, await MapToResponseAsync(jobCard));
    }

    /// <summary>
    /// Create job card with media (audio and images)
    /// </summary>
    [HttpPost("with-media")]
    public async Task<ActionResult<JobCardResponse>> CreateJobCardWithMedia(
        [FromForm] CreateJobCardRequest request,
        IFormFile? audioFile,
        List<IFormFile>? images,
        List<IFormFile>? videos)
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;

        // Verify vehicle exists
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Get staff name if staff is assigned
        string? staffName = null;
        if (request.AssignedStaffId.HasValue)
        {
            var staff = await _staffRepository.GetByIdAsync(request.AssignedStaffId.Value);
            staffName = staff?.Name;
        }

        // Upload audio file
        string? audioUrl = null;
        if (audioFile != null && audioFile.Length > 0)
        {
            using var stream = audioFile.OpenReadStream();
            audioUrl = await _fileUploadService.UploadFileAsync(stream, audioFile.FileName, $"jobcards/{workshopOwnerId}/audio");
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
                    var url = await _fileUploadService.UploadFileAsync(stream, image.FileName, $"jobcards/{workshopOwnerId}/images");
                    imageUrls.Add(url);
                }
            }
        }

        // Upload videos
        var videoUrls = new List<string>();
        if (videos != null)
        {
            foreach (var video in videos)
            {
                if (video.Length > 0)
                {
                    using var stream = video.OpenReadStream();
                    var url = await _fileUploadService.UploadFileAsync(stream, video.FileName, $"jobcards/{workshopOwnerId}/videos");
                    videoUrls.Add(url);
                }
            }
        }

        // Parse priority
        if (!Enum.TryParse<JobPriority>(request.Priority, true, out var priority))
        {
            priority = JobPriority.Normal;
        }

        var jobCard = new JobCard
        {
            VehicleId = request.VehicleId,
            VehicleVisitId = request.VehicleVisitId,
            WorkshopOwnerId = workshopOwnerId,
            JobCategory = request.JobCategory,
            AssignedStaffId = request.AssignedStaffId,
            AssignedStaffName = staffName,
            Remark = request.Remark,
            AudioUrl = audioUrl,
            Images = imageUrls.Count > 0 ? JsonSerializer.Serialize(imageUrls) : null,
            Videos = videoUrls.Count > 0 ? JsonSerializer.Serialize(videoUrls) : null,
            Status = JobCardStatus.Pending,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _jobCardRepository.CreateAsync(jobCard);
        jobCard.Id = id;

        _logger.LogInformation("Job card with media created: {Id} for vehicle {VehicleId}", id, request.VehicleId);

        return CreatedAtAction(nameof(GetJobCard), new { id }, await MapToResponseAsync(jobCard));
    }

    /// <summary>
    /// Get job card by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JobCardResponse>> GetJobCard(int id)
    {
        var jobCard = await _jobCardRepository.GetByIdAsync(id);
        if (jobCard == null)
        {
            return NotFound(new { message = "Job card not found" });
        }

        return Ok(await MapToResponseAsync(jobCard));
    }

    /// <summary>
    /// Get all job cards for a vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    public async Task<ActionResult<JobCardListResponse>> GetJobCardsByVehicle(int vehicleId)
    {
        var jobCards = await _jobCardRepository.GetByVehicleIdAsync(vehicleId);
        
        var responses = new List<JobCardResponse>();
        foreach (var jobCard in jobCards)
        {
            responses.Add(await MapToResponseAsync(jobCard));
        }

        return Ok(new JobCardListResponse(responses, responses.Count));
    }

    /// <summary>
    /// Get all job cards for the workshop
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<JobCardListResponse>> GetJobCards()
    {
        var workshopOwnerId = GetWorkshopOwnerIdFromToken() ?? 1;
        var jobCards = await _jobCardRepository.GetByWorkshopOwnerIdAsync(workshopOwnerId);
        
        var responses = new List<JobCardResponse>();
        foreach (var jobCard in jobCards)
        {
            responses.Add(await MapToResponseAsync(jobCard));
        }

        return Ok(new JobCardListResponse(responses, responses.Count));
    }

    /// <summary>
    /// Update job card
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<JobCardResponse>> UpdateJobCard(int id, [FromBody] UpdateJobCardRequest request)
    {
        var jobCard = await _jobCardRepository.GetByIdAsync(id);
        if (jobCard == null)
        {
            return NotFound(new { message = "Job card not found" });
        }

        // Update fields
        if (request.JobCategory != null) jobCard.JobCategory = request.JobCategory;
        if (request.Remark != null) jobCard.Remark = request.Remark;
        if (request.EstimatedCost.HasValue) jobCard.EstimatedCost = request.EstimatedCost.Value;
        if (request.ActualCost.HasValue) jobCard.ActualCost = request.ActualCost.Value;
        if (request.EstimatedDuration.HasValue) jobCard.EstimatedDuration = request.EstimatedDuration.Value;

        // Update staff assignment
        if (request.AssignedStaffId.HasValue)
        {
            var staff = await _staffRepository.GetByIdAsync(request.AssignedStaffId.Value);
            jobCard.AssignedStaffId = request.AssignedStaffId.Value;
            jobCard.AssignedStaffName = staff?.Name;
        }

        // Update status
        if (request.Status != null && Enum.TryParse<JobCardStatus>(request.Status, true, out var status))
        {
            jobCard.Status = status;
            if (status == JobCardStatus.InProgress && !jobCard.StartedAt.HasValue)
            {
                jobCard.StartedAt = DateTime.UtcNow;
            }
            else if (status == JobCardStatus.Completed)
            {
                jobCard.CompletedAt = DateTime.UtcNow;
            }
        }

        // Update priority
        if (request.Priority != null && Enum.TryParse<JobPriority>(request.Priority, true, out var priority))
        {
            jobCard.Priority = priority;
        }

        jobCard.UpdatedAt = DateTime.UtcNow;

        await _jobCardRepository.UpdateAsync(jobCard);

        _logger.LogInformation("Job card updated: {Id}", id);

        return Ok(await MapToResponseAsync(jobCard));
    }

    /// <summary>
    /// Delete job card
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJobCard(int id)
    {
        var jobCard = await _jobCardRepository.GetByIdAsync(id);
        if (jobCard == null)
        {
            return NotFound(new { message = "Job card not found" });
        }

        await _jobCardRepository.DeleteAsync(id);

        _logger.LogInformation("Job card deleted: {Id}", id);

        return Ok(new { message = "Job card deleted successfully" });
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

    private async Task<JobCardResponse> MapToResponseAsync(JobCard jobCard)
    {
        // Get vehicle info
        var vehicle = await _vehicleRepository.GetByIdAsync(jobCard.VehicleId);
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

        // Parse image JSON array
        List<string>? images = null;
        if (!string.IsNullOrEmpty(jobCard.Images))
        {
            try
            {
                images = JsonSerializer.Deserialize<List<string>>(jobCard.Images);
            }
            catch { }
        }

        // Parse video JSON array
        List<string>? videos = null;
        if (!string.IsNullOrEmpty(jobCard.Videos))
        {
            try
            {
                videos = JsonSerializer.Deserialize<List<string>>(jobCard.Videos);
            }
            catch { }
        }

        return new JobCardResponse(
            jobCard.Id,
            jobCard.VehicleId,
            jobCard.VehicleVisitId,
            jobCard.WorkshopOwnerId,
            jobCard.JobCategory,
            jobCard.AssignedStaffId,
            jobCard.AssignedStaffName,
            jobCard.Remark,
            jobCard.AudioUrl,
            images,
            videos,
            jobCard.Status.ToString(),
            jobCard.Priority.ToString(),
            jobCard.EstimatedCost,
            jobCard.ActualCost,
            jobCard.EstimatedDuration,
            jobCard.StartedAt,
            jobCard.CompletedAt,
            jobCard.CreatedAt,
            jobCard.UpdatedAt,
            vehicleInfo
        );
    }
}
