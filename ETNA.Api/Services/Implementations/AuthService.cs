using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IWorkshopOwnerRepository _ownerRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        IWorkshopOwnerRepository ownerRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _ownerRepository = ownerRepository;
        _jwtService = jwtService;
        _logger = logger;
    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            // Find user by email
            var owner = await _ownerRepository.GetByEmailAsync(dto.Email.ToLower().Trim());
            
            if (owner == null)
            {
                _logger.LogWarning("Login failed: Email not found - {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            // Verify password
            if (!VerifyPassword(dto.Password, owner.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            // Check if email is verified
            if (!owner.IsEmailVerified)
            {
                _logger.LogWarning("Login failed: Email not verified for {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please verify your email before logging in"
                };
            }
            
            // Check registration status
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Rejected)
            {
                _logger.LogWarning("Login failed: Account rejected for {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your registration has been rejected"
                };
            }
            
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Suspended)
            {
                _logger.LogWarning("Login failed: Account suspended for {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended"
                };
            }
            
            // Generate JWT token (valid for 30 days)
            var token = _jwtService.GenerateToken(owner, "Owner");
            var expiresAt = _jwtService.GetTokenExpiry();
            
            _logger.LogInformation("Login successful for {Email}", dto.Email);
            
            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = owner.Id,
                    Name = owner.OwnerName,
                    Email = owner.Email,
                    Role = "Owner",
                    WorkshopName = owner.WorkshopName,
                    City = owner.City,
                    IsEmailVerified = owner.IsEmailVerified,
                    IsActive = owner.IsActive,
                    RegistrationStatus = owner.RegistrationStatus.ToString()
                },
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", dto.Email);
            return new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }
    
    public async Task<ServiceResult<bool>> ValidateUserCanLoginAsync(string email)
    {
        var owner = await _ownerRepository.GetByEmailAsync(email.ToLower().Trim());
        
        if (owner == null)
        {
            return ServiceResult<bool>.FailureResult("User not found");
        }
        
        if (!owner.IsEmailVerified)
        {
            return ServiceResult<bool>.FailureResult("Email not verified");
        }
        
        if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Rejected)
        {
            return ServiceResult<bool>.FailureResult("Account rejected");
        }
        
        if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Suspended)
        {
            return ServiceResult<bool>.FailureResult("Account suspended");
        }
        
        return ServiceResult<bool>.SuccessResult(true);
    }
    
    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
