using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;
    private readonly ILogger<VerificationController> _logger;
    
    public VerificationController(
        IVerificationService verificationService,
        ILogger<VerificationController> logger)
    {
        _verificationService = verificationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Step 1: Initiate ETNA verification - sends OTP to owner's email
    /// </summary>
    [HttpPost("etna/initiate")]
    public async Task<IActionResult> InitiateETNAVerification([FromBody] InitiateETNAVerificationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _verificationService.InitiateETNAVerificationAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message });
    }
    
    /// <summary>
    /// Step 2: Complete ETNA verification with both OTPs
    /// </summary>
    [HttpPost("etna/complete")]
    public async Task<IActionResult> CompleteETNAVerification([FromBody] ETNAVerificationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _verificationService.CompleteETNAVerificationAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Step 3a: Upload owner photo
    /// </summary>
    [HttpPost("photos/owner")]
    public async Task<IActionResult> UploadOwnerPhoto([FromQuery] string email, IFormFile file)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { error = "Email is required" });
        }
        
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }
        
        using var stream = file.OpenReadStream();
        var result = await _verificationService.UploadOwnerPhotoAsync(email, stream, file.FileName);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Step 3b: Upload workshop photo
    /// </summary>
    [HttpPost("photos/workshop")]
    public async Task<IActionResult> UploadWorkshopPhoto([FromQuery] string email, IFormFile file)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { error = "Email is required" });
        }
        
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }
        
        using var stream = file.OpenReadStream();
        var result = await _verificationService.UploadWorkshopPhotoAsync(email, stream, file.FileName);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Step 4: Complete photo upload and activate account
    /// </summary>
    [HttpPost("photos/complete")]
    public async Task<IActionResult> CompletePhotoUpload([FromBody] PhotoUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _verificationService.CompletePhotoUploadAsync(dto.OwnerEmail);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
}
