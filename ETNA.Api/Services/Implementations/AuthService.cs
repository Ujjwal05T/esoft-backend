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
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        IWorkshopOwnerRepository ownerRepository,
        IWorkshopStaffRepository staffRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _ownerRepository = ownerRepository;
        _staffRepository = staffRepository;
        _jwtService = jwtService;
        _logger = logger;
    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        // Try owner login first
        var ownerResult = await LoginOwnerAsync(dto);
        if (ownerResult.Success)
        {
            return ownerResult;
        }
        
        // Try staff login
        var staffResult = await LoginStaffAsync(dto);
        return staffResult;
    }
    
    public async Task<LoginResponseDto> LoginOwnerAsync(LoginDto dto)
    {
        try
        {
            var owner = await _ownerRepository.GetByEmailAsync(dto.Email.ToLower().Trim());
            
            if (owner == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            if (!VerifyPassword(dto.Password, owner.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for owner {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            if (!owner.IsEmailVerified)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please verify your email before logging in"
                };
            }
            
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Rejected)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your registration has been rejected"
                };
            }
            
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Suspended)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended"
                };
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.Active)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Your registration is not complete. Status: {owner.RegistrationStatus}"
                };
            }
            
            var token = _jwtService.GenerateToken(owner, "Owner");
            var expiresAt = _jwtService.GetTokenExpiry();
            
            _logger.LogInformation("Owner login successful: {Email}", dto.Email);
            
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
            _logger.LogError(ex, "Error during owner login: {Email}", dto.Email);
            return new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }
    
    public async Task<LoginResponseDto> LoginStaffAsync(LoginDto dto)
    {
        try
        {
            var staff = await _staffRepository.GetByEmailAsync(dto.Email.ToLower().Trim());
            
            if (staff == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            if (!VerifyPassword(dto.Password, staff.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for staff {Email}", dto.Email);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            if (!staff.IsPhoneVerified)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please verify your phone before logging in"
                };
            }
            
            if (staff.RegistrationStatus == StaffRegistrationStatus.PendingOwnerApproval)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your registration is pending workshop owner approval"
                };
            }
            
            if (staff.RegistrationStatus == StaffRegistrationStatus.Rejected)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your registration request was rejected"
                };
            }
            
            if (staff.RegistrationStatus == StaffRegistrationStatus.Suspended)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended"
                };
            }
            
            if (staff.RegistrationStatus != StaffRegistrationStatus.Approved)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Your registration is not complete. Status: {staff.RegistrationStatus}"
                };
            }
            
            // Get workshop name
            var workshop = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            var workshopName = workshop?.WorkshopName ?? "Unknown Workshop";
            
            var token = _jwtService.GenerateStaffToken(staff, workshopName, "Staff");
            var expiresAt = _jwtService.GetTokenExpiry();
            
            _logger.LogInformation("Staff login successful: {Email}", dto.Email);
            
            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = staff.Id,
                    Name = staff.Name,
                    Email = staff.Email,
                    Role = "Staff",
                    WorkshopName = workshopName,
                    City = staff.City,
                    IsEmailVerified = staff.IsPhoneVerified,
                    IsActive = staff.IsActive,
                    RegistrationStatus = staff.RegistrationStatus.ToString()
                },
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during staff login: {Email}", dto.Email);
            return new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }
    
    public async Task<ServiceResult<bool>> ValidateUserCanLoginAsync(string email)
    {
        // Check owner first
        var owner = await _ownerRepository.GetByEmailAsync(email.ToLower().Trim());
        if (owner != null)
        {
            if (!owner.IsEmailVerified)
                return ServiceResult<bool>.FailureResult("Email not verified");
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Rejected)
                return ServiceResult<bool>.FailureResult("Account rejected");
            if (owner.RegistrationStatus == WorkshopOwnerRegistrationStatus.Suspended)
                return ServiceResult<bool>.FailureResult("Account suspended");
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.Active)
                return ServiceResult<bool>.FailureResult("Registration not complete");
            return ServiceResult<bool>.SuccessResult(true);
        }
        
        // Check staff
        var staff = await _staffRepository.GetByEmailAsync(email.ToLower().Trim());
        if (staff != null)
        {
            if (!staff.IsPhoneVerified)
                return ServiceResult<bool>.FailureResult("Phone not verified");
            if (staff.RegistrationStatus == StaffRegistrationStatus.PendingOwnerApproval)
                return ServiceResult<bool>.FailureResult("Pending owner approval");
            if (staff.RegistrationStatus == StaffRegistrationStatus.Rejected)
                return ServiceResult<bool>.FailureResult("Registration rejected");
            if (staff.RegistrationStatus == StaffRegistrationStatus.Suspended)
                return ServiceResult<bool>.FailureResult("Account suspended");
            if (staff.RegistrationStatus != StaffRegistrationStatus.Approved)
                return ServiceResult<bool>.FailureResult("Registration not complete");
            return ServiceResult<bool>.SuccessResult(true);
        }
        
        return ServiceResult<bool>.FailureResult("User not found");
    }
    
    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
