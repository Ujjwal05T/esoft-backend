using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Services.Interfaces;
using ETNA.Api.Models.DTOs;
using BCrypt.Net;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IWorkshopOwnerRepository _ownerRepository;
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IWorkshopOwnerRepository ownerRepository,
        IWorkshopStaffRepository staffRepository,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _ownerRepository = ownerRepository;
        _staffRepository = staffRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Login for workshop owner
    /// </summary>
    [HttpPost("login/owner")]
    public async Task<ActionResult<LoginResponse>> LoginOwner([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Phone number and password are required" });
            }

            // Find owner by phone number
            var owner = await _ownerRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            
            if (owner == null)
            {
                return Unauthorized(new { message = "Invalid phone number or password" });
            }

            // Check if account is active
            if (owner.RegistrationStatus != Models.Entities.RegistrationStatus.Active)
            {
                return Unauthorized(new { message = "Account is not active. Please wait for approval." });
            }

            // Verify password
            if (string.IsNullOrEmpty(owner.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, owner.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid phone number or password" });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(owner, "owner");
            var expiry = _jwtService.GetTokenExpiry();

            _logger.LogInformation("Owner {OwnerId} logged in successfully", owner.Id);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = expiry,
                User = new UserInfo
                {
                    Id = owner.Id,
                    Name = owner.OwnerName,
                    Email = owner.Email,
                    PhoneNumber = owner.PhoneNumber,
                    Role = "owner",
                    WorkshopName = owner.WorkshopName,
                    City = owner.City
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during owner login");
            return StatusCode(500, new { message = "Login failed. Please try again." });
        }
    }

    /// <summary>
    /// Login for workshop staff
    /// </summary>
    [HttpPost("login/staff")]
    public async Task<ActionResult<LoginResponse>> LoginStaff([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Phone number and password are required" });
            }

            // Find staff by phone number
            var staff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            
            if (staff == null)
            {
                return Unauthorized(new { message = "Invalid phone number or password" });
            }

            // Check if account is active
            if (!staff.IsActive)
            {
                return Unauthorized(new { message = "Account is inactive. Please contact your workshop owner." });
            }

            // Verify password
            if (string.IsNullOrEmpty(staff.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid phone number or password" });
            }

            // Get workshop owner info for workshop name
            var owner = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            var workshopName = owner?.WorkshopName ?? "Unknown Workshop";

            // Generate JWT token
            var token = _jwtService.GenerateStaffToken(staff, workshopName, "staff");
            var expiry = _jwtService.GetTokenExpiry();

            _logger.LogInformation("Staff {StaffId} logged in successfully", staff.Id);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = expiry,
                User = new UserInfo
                {
                    Id = staff.Id,
                    Name = staff.Name,
                    Email = staff.Email,
                    PhoneNumber = staff.PhoneNumber,
                    Role = "staff",
                    WorkshopName = workshopName,
                    City = staff.City
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during staff login");
            return StatusCode(500, new { message = "Login failed. Please try again." });
        }
    }

    /// <summary>
    /// Generic login (auto-detect owner or staff)
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Phone number and password are required" });
            }

            // Try owner first
            var owner = await _ownerRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (owner != null)
            {
                if (owner.RegistrationStatus != Models.Entities.RegistrationStatus.Active)
                {
                    return Unauthorized(new { message = "Account is not active. Please wait for approval." });
                }

                if (!string.IsNullOrEmpty(owner.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, owner.PasswordHash))
                {
                    var token = _jwtService.GenerateToken(owner, "owner");
                    var expiry = _jwtService.GetTokenExpiry();

                    _logger.LogInformation("Owner {OwnerId} logged in successfully", owner.Id);

                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        Token = token,
                        ExpiresAt = expiry,
                        User = new UserInfo
                        {
                            Id = owner.Id,
                            Name = owner.OwnerName,
                            Email = owner.Email,
                            PhoneNumber = owner.PhoneNumber,
                            Role = "owner",
                            WorkshopName = owner.WorkshopName,
                            City = owner.City
                        }
                    });
                }
            }

            // Try staff
            var staff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (staff != null)
            {
                if (!staff.IsActive)
                {
                    return Unauthorized(new { message = "Account is inactive. Please contact your workshop owner." });
                }

                if (!string.IsNullOrEmpty(staff.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
                {
                    var workshopOwner = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
                    var workshopName = workshopOwner?.WorkshopName ?? "Unknown Workshop";

                    var token = _jwtService.GenerateStaffToken(staff, workshopName, "staff");
                    var expiry = _jwtService.GetTokenExpiry();

                    _logger.LogInformation("Staff {StaffId} logged in successfully", staff.Id);

                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        Token = token,
                        ExpiresAt = expiry,
                        User = new UserInfo
                        {
                            Id = staff.Id,
                            Name = staff.Name,
                            Email = staff.Email,
                            PhoneNumber = staff.PhoneNumber,
                            Role = "staff",
                            WorkshopName = workshopName,
                            City = staff.City
                        }
                    });
                }
            }

            return Unauthorized(new { message = "Invalid phone number or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Login failed. Please try again." });
        }
    }
}
