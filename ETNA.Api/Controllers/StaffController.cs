using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Controllers;

/// <summary>
/// API Controller for Workshop Staff management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(
        IWorkshopStaffRepository staffRepository,
        IFileUploadService fileUploadService,
        ILogger<StaffController> logger)
    {
        _staffRepository = staffRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Get all staff members for the workshop
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StaffListResponse>> GetAllStaff()
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByWorkshopOwnerIdAsync(workshopOwnerId.Value);
            var staffList = staff.ToList();
            
            var response = new StaffListResponse
            {
                Staff = staffList.Select(MapToResponse).ToList(),
                TotalCount = staffList.Count,
                ActiveCount = staffList.Count(s => s.IsActive),
                InactiveCount = staffList.Count(s => !s.IsActive)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff list");
            return StatusCode(500, new { message = "Error retrieving staff list" });
        }
    }

    /// <summary>
    /// Get active staff members only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<StaffListResponse>> GetActiveStaff()
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetActiveByWorkshopOwnerIdAsync(workshopOwnerId.Value);
            var staffList = staff.ToList();
            
            var totalCount = await _staffRepository.GetTotalCountByWorkshopOwnerIdAsync(workshopOwnerId.Value);
            
            var response = new StaffListResponse
            {
                Staff = staffList.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                ActiveCount = staffList.Count,
                InactiveCount = totalCount - staffList.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active staff list");
            return StatusCode(500, new { message = "Error retrieving active staff list" });
        }
    }

    /// <summary>
    /// Get inactive staff members only
    /// </summary>
    [HttpGet("inactive")]
    public async Task<ActionResult<StaffListResponse>> GetInactiveStaff()
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetInactiveByWorkshopOwnerIdAsync(workshopOwnerId.Value);
            var staffList = staff.ToList();
            
            var totalCount = await _staffRepository.GetTotalCountByWorkshopOwnerIdAsync(workshopOwnerId.Value);
            
            var response = new StaffListResponse
            {
                Staff = staffList.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                ActiveCount = totalCount - staffList.Count,
                InactiveCount = staffList.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inactive staff list");
            return StatusCode(500, new { message = "Error retrieving inactive staff list" });
        }
    }

    /// <summary>
    /// Get a specific staff member by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StaffResponse>> GetStaff(int id)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            // Verify the staff belongs to this workshop owner
            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            return Ok(MapToResponse(staff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff member {Id}", id);
            return StatusCode(500, new { message = "Error retrieving staff member" });
        }
    }

    /// <summary>
    /// Create a new staff member
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StaffResponse>> CreateStaff([FromBody] CreateStaffRequest request)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            // Check if phone number already exists
            var existingStaff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingStaff != null)
                return BadRequest(new { message = "A staff member with this phone number already exists" });

            var staff = new WorkshopStaff
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Address = request.Address,
                WorkshopOwnerId = workshopOwnerId.Value,
                WorkshopId = workshopOwnerId.Value, // Default to same as owner ID
                Role = request.Role,
                JobCategories = request.JobCategories != null ? JsonSerializer.Serialize(request.JobCategories) : null,
                CanApproveVehicles = request.CanApproveVehicles,
                CanApproveInquiries = request.CanApproveInquiries,
                CanGenerateEstimates = request.CanGenerateEstimates,
                CanCreateJobCard = request.CanCreateJobCard,
                CanApproveDisputes = request.CanApproveDisputes,
                CanApproveQuotesPayments = request.CanApproveQuotesPayments,
                IsActive = true,
                IsPhoneVerified = false,
                RegistrationStatus = RegistrationStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var createdStaff = await _staffRepository.CreateAsync(staff);
            
            _logger.LogInformation("Staff member {Name} created with ID {Id}", createdStaff.Name, createdStaff.Id);
            
            return CreatedAtAction(nameof(GetStaff), new { id = createdStaff.Id }, MapToResponse(createdStaff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member");
            return StatusCode(500, new { message = "Error creating staff member" });
        }
    }

    /// <summary>
    /// Create a new staff member with photo upload
    /// </summary>
    [HttpPost("with-photo")]
    public async Task<ActionResult<StaffResponse>> CreateStaffWithPhoto([FromForm] CreateStaffRequest request, IFormFile? photo)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            // Check if phone number already exists
            var existingStaff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingStaff != null)
                return BadRequest(new { message = "A staff member with this phone number already exists" });

            string? photoUrl = null;
            if (photo != null)
            {
                using var stream = photo.OpenReadStream();
                photoUrl = await _fileUploadService.UploadFileAsync(stream, photo.FileName, "staff-photos");
            }

            var staff = new WorkshopStaff
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Address = request.Address,
                PhotoUrl = photoUrl,
                WorkshopOwnerId = workshopOwnerId.Value,
                WorkshopId = workshopOwnerId.Value,
                Role = request.Role,
                JobCategories = request.JobCategories != null ? JsonSerializer.Serialize(request.JobCategories) : null,
                CanApproveVehicles = request.CanApproveVehicles,
                CanApproveInquiries = request.CanApproveInquiries,
                CanGenerateEstimates = request.CanGenerateEstimates,
                CanCreateJobCard = request.CanCreateJobCard,
                CanApproveDisputes = request.CanApproveDisputes,
                CanApproveQuotesPayments = request.CanApproveQuotesPayments,
                IsActive = true,
                IsPhoneVerified = false,
                RegistrationStatus = RegistrationStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var createdStaff = await _staffRepository.CreateAsync(staff);
            
            _logger.LogInformation("Staff member {Name} created with photo, ID {Id}", createdStaff.Name, createdStaff.Id);
            
            return CreatedAtAction(nameof(GetStaff), new { id = createdStaff.Id }, MapToResponse(createdStaff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member with photo");
            return StatusCode(500, new { message = "Error creating staff member" });
        }
    }

    /// <summary>
    /// Update a staff member
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StaffResponse>> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name)) staff.Name = request.Name;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) staff.PhoneNumber = request.PhoneNumber;
            if (request.Email != null) staff.Email = request.Email;
            if (request.Address != null) staff.Address = request.Address;
            if (!string.IsNullOrEmpty(request.Role)) staff.Role = request.Role;
            if (request.JobCategories != null) staff.JobCategories = JsonSerializer.Serialize(request.JobCategories);
            
            // Update permissions if provided
            if (request.CanApproveVehicles.HasValue) staff.CanApproveVehicles = request.CanApproveVehicles.Value;
            if (request.CanApproveInquiries.HasValue) staff.CanApproveInquiries = request.CanApproveInquiries.Value;
            if (request.CanGenerateEstimates.HasValue) staff.CanGenerateEstimates = request.CanGenerateEstimates.Value;
            if (request.CanCreateJobCard.HasValue) staff.CanCreateJobCard = request.CanCreateJobCard.Value;
            if (request.CanApproveDisputes.HasValue) staff.CanApproveDisputes = request.CanApproveDisputes.Value;
            if (request.CanApproveQuotesPayments.HasValue) staff.CanApproveQuotesPayments = request.CanApproveQuotesPayments.Value;

            var updatedStaff = await _staffRepository.UpdateAsync(staff);
            
            if (updatedStaff == null)
                return StatusCode(500, new { message = "Error updating staff member" });

            _logger.LogInformation("Staff member {Id} updated", id);
            
            return Ok(MapToResponse(updatedStaff));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff member {Id}", id);
            return StatusCode(500, new { message = "Error updating staff member" });
        }
    }

    /// <summary>
    /// Update staff permissions only
    /// </summary>
    [HttpPatch("{id}/permissions")]
    public async Task<ActionResult> UpdateStaffPermissions(int id, [FromBody] UpdateStaffPermissionsRequest request)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            var success = await _staffRepository.UpdatePermissionsAsync(
                id,
                request.CanApproveVehicles,
                request.CanApproveInquiries,
                request.CanGenerateEstimates,
                request.CanCreateJobCard,
                request.CanApproveDisputes,
                request.CanApproveQuotesPayments
            );

            if (!success)
                return StatusCode(500, new { message = "Error updating permissions" });

            _logger.LogInformation("Staff member {Id} permissions updated", id);
            
            return Ok(new { message = "Permissions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff permissions {Id}", id);
            return StatusCode(500, new { message = "Error updating permissions" });
        }
    }

    /// <summary>
    /// Upload/update staff photo
    /// </summary>
    [HttpPost("{id}/photo")]
    public async Task<ActionResult> UploadStaffPhoto(int id, IFormFile photo)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            using var stream = photo.OpenReadStream();
            var photoUrl = await _fileUploadService.UploadFileAsync(stream, photo.FileName, "staff-photos");
            var success = await _staffRepository.UpdatePhotoAsync(id, photoUrl);

            if (!success)
                return StatusCode(500, new { message = "Error uploading photo" });

            _logger.LogInformation("Staff member {Id} photo updated", id);
            
            return Ok(new { photoUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading staff photo {Id}", id);
            return StatusCode(500, new { message = "Error uploading photo" });
        }
    }

    /// <summary>
    /// Set staff member as active
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult> ActivateStaff(int id)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            var success = await _staffRepository.SetActiveStatusAsync(id, true);

            if (!success)
                return StatusCode(500, new { message = "Error activating staff member" });

            _logger.LogInformation("Staff member {Id} activated", id);
            
            return Ok(new { message = "Staff member activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating staff member {Id}", id);
            return StatusCode(500, new { message = "Error activating staff member" });
        }
    }

    /// <summary>
    /// Set staff member as inactive
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult> DeactivateStaff(int id)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            var success = await _staffRepository.SetActiveStatusAsync(id, false);

            if (!success)
                return StatusCode(500, new { message = "Error deactivating staff member" });

            _logger.LogInformation("Staff member {Id} deactivated", id);
            
            return Ok(new { message = "Staff member deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating staff member {Id}", id);
            return StatusCode(500, new { message = "Error deactivating staff member" });
        }
    }

    /// <summary>
    /// Delete a staff member
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStaff(int id)
    {
        try
        {
            var workshopOwnerId = GetWorkshopOwnerIdFromToken();
            if (workshopOwnerId == null)
                return Unauthorized(new { message = "Invalid token" });

            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
                return NotFound(new { message = "Staff member not found" });

            if (staff.WorkshopOwnerId != workshopOwnerId.Value)
                return Forbid();

            var success = await _staffRepository.DeleteAsync(id);

            if (!success)
                return StatusCode(500, new { message = "Error deleting staff member" });

            _logger.LogInformation("Staff member {Id} deleted", id);
            
            return Ok(new { message = "Staff member deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff member {Id}", id);
            return StatusCode(500, new { message = "Error deleting staff member" });
        }
    }

    // Helper method to get WorkshopOwnerId from JWT token
    private int? GetWorkshopOwnerIdFromToken()
    {
        var workshopOwnerIdClaim = User.FindFirst("WorkshopOwnerId")?.Value;
        if (int.TryParse(workshopOwnerIdClaim, out int workshopOwnerId))
        {
            return workshopOwnerId;
        }
        return null;
    }

    // Helper method to map entity to response DTO
    private static StaffResponse MapToResponse(WorkshopStaff staff)
    {
        string[]? jobCategories = null;
        if (!string.IsNullOrEmpty(staff.JobCategories))
        {
            try
            {
                jobCategories = JsonSerializer.Deserialize<string[]>(staff.JobCategories);
            }
            catch
            {
                jobCategories = null;
            }
        }

        return new StaffResponse
        {
            Id = staff.Id,
            Name = staff.Name,
            PhoneNumber = staff.PhoneNumber,
            Email = staff.Email,
            Address = staff.Address,
            PhotoUrl = staff.PhotoUrl,
            WorkshopId = staff.WorkshopId,
            WorkshopOwnerId = staff.WorkshopOwnerId,
            City = staff.City,
            Role = staff.Role,
            JobCategories = jobCategories,
            Permissions = new StaffPermissionsResponse
            {
                VehicleApprovals = staff.CanApproveVehicles,
                InquiryApprovals = staff.CanApproveInquiries,
                GenerateEstimates = staff.CanGenerateEstimates,
                CreateJobCard = staff.CanCreateJobCard,
                DisputeApprovals = staff.CanApproveDisputes,
                QuoteApprovalsPayments = staff.CanApproveQuotesPayments
            },
            IsActive = staff.IsActive,
            IsPhoneVerified = staff.IsPhoneVerified,
            RegistrationStatus = staff.RegistrationStatus.ToString(),
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt
        };
    }
}
