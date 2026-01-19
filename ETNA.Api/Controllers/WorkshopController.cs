using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/workshop")]
public class WorkshopController : ControllerBase
{
    private readonly IWorkshopRegistrationService _registrationService;
    private readonly IFileUploadService _fileUploadService;

    public WorkshopController(
        IWorkshopRegistrationService registrationService,
        IFileUploadService fileUploadService)
    {
        _registrationService = registrationService;
        _fileUploadService = fileUploadService;
    }

    // ==================== Frontend (Mobile App) - Workshop Owner Registration ====================

    /// <summary>
    /// Send OTP for phone verification during registration
    /// </summary>
    [HttpPost("register/send-otp")]
    public async Task<IActionResult> SendRegistrationOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) || request.PhoneNumber.Length < 10)
        {
            return BadRequest(new ApiResponse(false, "Please provide a valid phone number."));
        }

        var result = await _registrationService.SendRegistrationOtpAsync(request.PhoneNumber);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Verify OTP during registration
    /// </summary>
    [HttpPost("register/verify-otp")]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Otp))
        {
            return BadRequest(new ApiResponse(false, "Phone number and OTP are required."));
        }

        var result = await _registrationService.VerifyRegistrationOtpAsync(request.PhoneNumber, request.Otp);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Submit workshop registration request
    /// </summary>
    [HttpPost("register/submit")]
    public async Task<IActionResult> SubmitRegistration([FromBody] WorkshopRegistrationRequest request)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(request.OwnerName) ||
            string.IsNullOrEmpty(request.PhoneNumber) ||
            string.IsNullOrEmpty(request.AadhaarNumber) ||
            string.IsNullOrEmpty(request.WorkshopName) ||
            string.IsNullOrEmpty(request.Address) ||
            string.IsNullOrEmpty(request.PinCode) ||
            string.IsNullOrEmpty(request.City))
        {
            return BadRequest(new ApiResponse(false, "Please fill in all required fields."));
        }

        var result = await _registrationService.SubmitRegistrationAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

[ApiController]
[Route("api/admin/workshops")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is implemented
public class AdminWorkshopController : ControllerBase
{
    private readonly IWorkshopRegistrationService _registrationService;
    private readonly IFileUploadService _fileUploadService;

    public AdminWorkshopController(
        IWorkshopRegistrationService registrationService,
        IFileUploadService fileUploadService)
    {
        _registrationService = registrationService;
        _fileUploadService = fileUploadService;
    }

    // ==================== Frontend-Portal (Admin) - Workshop Management ====================

    /// <summary>
    /// Get all pending workshop requests
    /// </summary>
    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? source = null)
    {
        var result = await _registrationService.GetPendingRequestsAsync(page, pageSize, source);
        return Ok(new ApiResponse<WorkshopRequestsResponse>(true, "Pending requests retrieved.", result));
    }

    /// <summary>
    /// Get all onboarded (active) workshops
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOnboardedWorkshops()
    {
        var result = await _registrationService.GetOnboardedWorkshopsAsync();
        return Ok(new ApiResponse<List<WorkshopDto>>(true, "Onboarded workshops retrieved.", result));
    }

    /// <summary>
    /// Get workshop by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkshopById(int id)
    {
        var workshop = await _registrationService.GetWorkshopByIdAsync(id);
        
        if (workshop == null)
        {
            return NotFound(new ApiResponse(false, "Workshop not found."));
        }
        
        return Ok(new ApiResponse<object>(true, "Workshop retrieved.", workshop));
    }

    /// <summary>
    /// Send OTP to ETNA team verifier
    /// </summary>
    [HttpPost("verify/send-otp")]
    public async Task<IActionResult> SendEtnaVerificationOtp([FromBody] EtnaSendOtpRequest request)
    {
        if (request.WorkshopId <= 0 || string.IsNullOrEmpty(request.EtnaTeamMobileNumber))
        {
            return BadRequest(new ApiResponse(false, "Workshop ID and ETNA mobile number are required."));
        }

        var result = await _registrationService.SendEtnaVerificationOtpAsync(request.WorkshopId, request.EtnaTeamMobileNumber);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Verify ETNA team OTP
    /// </summary>
    [HttpPost("verify/verify-otp")]
    public async Task<IActionResult> VerifyEtnaOtp([FromBody] EtnaVerifyOtpRequest request)
    {
        if (request.WorkshopId <= 0 || 
            string.IsNullOrEmpty(request.EtnaTeamMobileNumber) || 
            string.IsNullOrEmpty(request.Otp))
        {
            return BadRequest(new ApiResponse(false, "All fields are required."));
        }

        var result = await _registrationService.VerifyEtnaOtpAsync(request.WorkshopId, request.EtnaTeamMobileNumber, request.Otp);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Upload documents for workshop
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<IActionResult> UploadDocuments(
        int id,
        IFormFile? tradeLicense,
        IFormFile? workshopPhoto,
        IFormFile? ownerPhoto)
    {
        var workshop = await _registrationService.GetWorkshopByIdAsync(id);
        if (workshop == null)
        {
            return NotFound(new ApiResponse(false, "Workshop not found."));
        }

        string? tradeLicenseUrl = null;
        string? workshopPhotoUrl = null;
        string? ownerPhotoUrl = null;
        var folder = $"workshops/{id}";

        // Upload trade license
        if (tradeLicense != null)
        {
            using var stream = tradeLicense.OpenReadStream();
            tradeLicenseUrl = await _fileUploadService.UploadFileAsync(stream, tradeLicense.FileName, folder);
        }

        // Upload workshop photo
        if (workshopPhoto != null)
        {
            using var stream = workshopPhoto.OpenReadStream();
            workshopPhotoUrl = await _fileUploadService.UploadFileAsync(stream, workshopPhoto.FileName, folder);
        }

        // Upload owner photo
        if (ownerPhoto != null)
        {
            using var stream = ownerPhoto.OpenReadStream();
            ownerPhotoUrl = await _fileUploadService.UploadFileAsync(stream, ownerPhoto.FileName, folder);
        }

        var result = await _registrationService.UploadDocumentsAsync(id, tradeLicenseUrl, workshopPhotoUrl, ownerPhotoUrl);
        
        return Ok(new ApiResponse<object>(true, "Documents uploaded successfully.", new
        {
            TradeLicenseUrl = tradeLicenseUrl,
            WorkshopPhotoUrl = workshopPhotoUrl,
            OwnerPhotoUrl = ownerPhotoUrl
        }));
    }

    /// <summary>
    /// Complete workshop onboarding
    /// </summary>
    [HttpPost("{id}/onboard")]
    public async Task<IActionResult> CompleteOnboarding(int id, [FromBody] CompleteOnboardingRequest request)
    {
        var result = await _registrationService.CompleteOnboardingAsync(id, request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Reject workshop request
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectWorkshop(int id, [FromBody] RejectWorkshopRequest request)
    {
        var result = await _registrationService.RejectWorkshopAsync(id, request.Reason);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

// Additional DTO for rejection
public record RejectWorkshopRequest(string Reason);
