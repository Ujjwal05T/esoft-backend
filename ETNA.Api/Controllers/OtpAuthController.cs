using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Services.Interfaces;
using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class OtpAuthController : ControllerBase
{
    private readonly IWorkshopOwnerRepository _ownerRepository;
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<OtpAuthController> _logger;

    public OtpAuthController(
        IWorkshopOwnerRepository ownerRepository,
        IWorkshopStaffRepository staffRepository,
        IJwtService jwtService,
        ILogger<OtpAuthController> logger)
    {
        _ownerRepository = ownerRepository;
        _staffRepository = staffRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Send OTP to mobile number (always returns success with hardcoded OTP 111111)
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<ActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest(new { message = "Phone number is required" });
            }

            // Check if user exists (owner or staff)
            var owner = await _ownerRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            var staff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);

            if (owner == null && staff == null)
            {
                return NotFound(new { message = "Phone number not registered" });
            }

            // In production, send actual OTP via SMS
            // For now, always use hardcoded OTP: 111111
            _logger.LogInformation("OTP requested for {PhoneNumber}. Using hardcoded OTP: 111111", request.PhoneNumber);

            return Ok(new 
            { 
                success = true, 
                message = "OTP sent successfully. Use 111111 to login.",
                phoneNumber = request.PhoneNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            return StatusCode(500, new { message = "Failed to send OTP. Please try again." });
        }
    }

    /// <summary>
    /// Verify OTP and login (hardcoded OTP: 111111)
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<ActionResult<LoginResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest(new { message = "Phone number and OTP are required" });
            }

            // Verify hardcoded OTP
            if (request.Otp != "111111")
            {
                return Unauthorized(new { message = "Invalid OTP" });
            }

            // Try owner first
            var owner = await _ownerRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (owner != null)
            {
                if (owner.RegistrationStatus != Models.Entities.RegistrationStatus.Active)
                {
                    return Unauthorized(new { message = "Account is not active. Please wait for approval." });
                }

                var token = _jwtService.GenerateToken(owner, "owner");
                var expiry = _jwtService.GetTokenExpiry();

                _logger.LogInformation("Owner {OwnerId} logged in via OTP", owner.Id);

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

            // Try staff
            var staff = await _staffRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (staff != null)
            {
                if (!staff.IsActive)
                {
                    return Unauthorized(new { message = "Account is inactive. Please contact your workshop owner." });
                }

                var workshopOwner = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
                var workshopName = workshopOwner?.WorkshopName ?? "Unknown Workshop";

                var token = _jwtService.GenerateStaffToken(staff, workshopName, "staff");
                var expiry = _jwtService.GetTokenExpiry();

                _logger.LogInformation("Staff {StaffId} logged in via OTP", staff.Id);

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

            return NotFound(new { message = "Phone number not registered" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            return StatusCode(500, new { message = "OTP verification failed. Please try again." });
        }
    }
}
