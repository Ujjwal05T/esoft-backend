using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkshopOwnerController : ControllerBase
{
    private readonly IWorkshopOwnerService _service;
    private readonly ILogger<WorkshopOwnerController> _logger;
    
    public WorkshopOwnerController(
        IWorkshopOwnerService service,
        ILogger<WorkshopOwnerController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new workshop owner
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] WorkshopOwnerRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _service.RegisterAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.Data!.Id }, 
            new { message = result.Message, data = result.Data });
    }
    
    /// <summary>
    /// Verify email with OTP
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _service.VerifyEmailAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message });
    }
    
    /// <summary>
    /// Resend OTP to email
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _service.ResendOtpAsync(dto.Email);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = result.Message });
    }
    
    /// <summary>
    /// Get workshop owner by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Get all active workshops (for staff dropdown)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveWorkshops()
    {
        var result = await _service.GetActiveWorkshopsAsync();
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
    
    /// <summary>
    /// Get workshops by city (for staff filtering)
    /// </summary>
    [HttpGet("by-city/{city}")]
    public async Task<IActionResult> GetByCity(string city)
    {
        var result = await _service.GetWorkshopsByCityAsync(city);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }
}
