using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _authService.LoginAsync(dto);
        
        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Validate if user can login
    /// </summary>
    [HttpGet("validate/{email}")]
    public async Task<IActionResult> ValidateUser(string email)
    {
        var result = await _authService.ValidateUserCanLoginAsync(email);
        
        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { canLogin = true });
    }
}
