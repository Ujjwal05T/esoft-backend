using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;
using System.Security.Claims;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkshopStaffController : ControllerBase
{
    private readonly IWorkshopStaffService _staffService;
    private readonly ILogger<WorkshopStaffController> _logger;
    
    public WorkshopStaffController(
        IWorkshopStaffService staffService,
        ILogger<WorkshopStaffController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get workshops by city (for dropdown selection)
    /// </summary>
    [HttpGet("workshops/{city}")]
    public async Task<IActionResult> GetWorkshopsByCity(string city)
    {
        var result = await _staffService.GetWorkshopsByCityAsync(city);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Register a new staff member
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] StaffRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _staffService.RegisterAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message, data = result.Data });
    }
    
    /// <summary>
    /// Verify phone with SMS OTP
    /// </summary>
    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] StaffVerifyPhoneDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _staffService.VerifyPhoneAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message, data = result.Data });
    }
    
    /// <summary>
    /// Resend OTP to phone
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendPhoneOtpDto dto)
    {
        var result = await _staffService.ResendOtpAsync(dto.PhoneNumber);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message });
    }
    
    /// <summary>
    /// Check registration status by email
    /// </summary>
    [HttpGet("status/{email}")]
    public async Task<IActionResult> CheckStatus(string email)
    {
        var result = await _staffService.CheckStatusAsync(email);
        
        if (!result.Success)
        {
            return NotFound(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Get staff by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _staffService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Get pending staff requests (for workshop owner)
    /// </summary>
    [HttpGet("pending/{workshopOwnerId}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetPendingRequests(int workshopOwnerId)
    {
        // Verify the requesting user is the owner
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != workshopOwnerId.ToString())
        {
            return Forbid();
        }
        
        var result = await _staffService.GetPendingRequestsAsync(workshopOwnerId);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Approve or reject staff request (for workshop owner)
    /// </summary>
    [HttpPost("approve")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ProcessApproval([FromBody] StaffApprovalDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var ownerId))
        {
            return Unauthorized();
        }
        
        var result = await _staffService.ProcessApprovalAsync(dto, ownerId);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message, data = result.Data });
    }
    
    /// <summary>
    /// Get all staff for a workshop (for workshop owner)
    /// </summary>
    [HttpGet("workshop/{workshopOwnerId}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetStaffByWorkshop(int workshopOwnerId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != workshopOwnerId.ToString())
        {
            return Forbid();
        }
        
        var result = await _staffService.GetStaffByWorkshopAsync(workshopOwnerId);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
}
