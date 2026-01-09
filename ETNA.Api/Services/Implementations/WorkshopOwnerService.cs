using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Service implementation for WorkshopOwner business logic
/// </summary>
public class WorkshopOwnerService : IWorkshopOwnerService
{
    private readonly IWorkshopOwnerRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<WorkshopOwnerService> _logger;
    
    private const string RegistrationPurpose = "Registration";
    
    public WorkshopOwnerService(
        IWorkshopOwnerRepository repository,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<WorkshopOwnerService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }
    
    public async Task<ServiceResult<WorkshopOwnerResponseDto>> RegisterAsync(WorkshopOwnerRegisterDto dto)
    {
        try
        {
            // Check if email already exists
            if (await _repository.EmailExistsAsync(dto.Email))
            {
                return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("Email already registered");
            }
            
            // Create new workshop owner entity
            var owner = new WorkshopOwner
            {
                OwnerName = dto.OwnerName,
                Email = dto.Email.ToLower().Trim(),
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = HashPassword(dto.Password),
                WorkshopName = dto.WorkshopName,
                Address = dto.Address,
                City = dto.City,
                TradeLicense = dto.TradeLicense,
                IsEmailVerified = false,
                IsActive = false,
                RegistrationStatus = WorkshopOwnerRegistrationStatus.PendingEmailVerification,
                CreatedAt = DateTime.UtcNow
            };
            
            // Save to database
            var id = await _repository.CreateAsync(owner);
            owner.Id = id;
            
            // Generate and send OTP email
            var otp = await _otpService.GenerateOtpAsync(owner.Email, RegistrationPurpose);
            var emailSent = await _emailService.SendOtpEmailAsync(owner.Email, otp, RegistrationPurpose);
            
            if (!emailSent)
            {
                _logger.LogWarning("Failed to send OTP email to {Email}, but registration continues", owner.Email);
            }
            
            _logger.LogInformation("Workshop owner registered successfully: {Email}", owner.Email);
            
            return ServiceResult<WorkshopOwnerResponseDto>.SuccessResult(
                MapToResponseDto(owner),
                "Registration successful. Please verify your email with the OTP sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering workshop owner: {Email}", dto.Email);
            return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("An error occurred during registration");
        }
    }
    
    public async Task<ServiceResult<WorkshopOwnerResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var owner = await _repository.GetByIdAsync(id);
            if (owner == null)
            {
                return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("Workshop owner not found");
            }
            
            return ServiceResult<WorkshopOwnerResponseDto>.SuccessResult(MapToResponseDto(owner));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workshop owner by id: {Id}", id);
            return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<WorkshopOwnerResponseDto>> GetByEmailAsync(string email)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(email.ToLower().Trim());
            if (owner == null)
            {
                return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("Workshop owner not found");
            }
            
            return ServiceResult<WorkshopOwnerResponseDto>.SuccessResult(MapToResponseDto(owner));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workshop owner by email: {Email}", email);
            return ServiceResult<WorkshopOwnerResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<bool>> VerifyEmailAsync(VerifyEmailDto dto)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(dto.Email.ToLower().Trim());
            if (owner == null)
            {
                return ServiceResult<bool>.FailureResult("Email not found");
            }
            
            if (owner.IsEmailVerified)
            {
                return ServiceResult<bool>.FailureResult("Email already verified");
            }
            
            // Validate OTP
            var isValidOtp = await _otpService.ValidateOtpAsync(dto.Email, dto.Otp, RegistrationPurpose);
            if (!isValidOtp)
            {
                return ServiceResult<bool>.FailureResult("Invalid or expired OTP");
            }
            
            // Invalidate OTP after successful verification
            await _otpService.InvalidateOtpAsync(dto.Email, RegistrationPurpose);
            
            // Update verification status
            await _repository.UpdateEmailVerificationAsync(
                owner.Id, 
                true, 
                WorkshopOwnerRegistrationStatus.PendingETNAVerification);
            
            _logger.LogInformation("Email verified for workshop owner: {Email}", dto.Email);
            
            return ServiceResult<bool>.SuccessResult(true, "Email verified successfully. Awaiting ETNA team verification.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email: {Email}", dto.Email);
            return ServiceResult<bool>.FailureResult("An error occurred during verification");
        }
    }
    
    public async Task<ServiceResult<bool>> ResendOtpAsync(string email)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(email.ToLower().Trim());
            if (owner == null)
            {
                return ServiceResult<bool>.FailureResult("Email not found");
            }
            
            if (owner.IsEmailVerified)
            {
                return ServiceResult<bool>.FailureResult("Email already verified");
            }
            
            // Generate and send new OTP
            var otp = await _otpService.GenerateOtpAsync(email, RegistrationPurpose);
            var emailSent = await _emailService.SendOtpEmailAsync(email, otp, RegistrationPurpose);
            
            if (!emailSent)
            {
                return ServiceResult<bool>.FailureResult("Failed to send OTP email. Please try again.");
            }
            
            _logger.LogInformation("OTP resent to: {Email}", email);
            
            return ServiceResult<bool>.SuccessResult(true, "OTP sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP: {Email}", email);
            return ServiceResult<bool>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>> GetActiveWorkshopsAsync()
    {
        try
        {
            var workshops = await _repository.GetActiveWorkshopsAsync();
            var response = workshops.Select(MapToResponseDto);
            return ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active workshops");
            return ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>> GetWorkshopsByCityAsync(string city)
    {
        try
        {
            var workshops = await _repository.GetByCityAsync(city);
            var response = workshops.Select(MapToResponseDto);
            return ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workshops by city: {City}", city);
            return ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>.FailureResult("An error occurred");
        }
    }
    
    // ===== Private Helper Methods =====
    
    private static string HashPassword(string password)
    {
        // Using BCrypt for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    private static WorkshopOwnerResponseDto MapToResponseDto(WorkshopOwner owner)
    {
        return new WorkshopOwnerResponseDto
        {
            Id = owner.Id,
            OwnerName = owner.OwnerName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,
            WorkshopName = owner.WorkshopName,
            Address = owner.Address,
            City = owner.City,
            TradeLicense = owner.TradeLicense,
            IsEmailVerified = owner.IsEmailVerified,
            IsActive = owner.IsActive,
            RegistrationStatus = owner.RegistrationStatus.ToString(),
            CreatedAt = owner.CreatedAt
        };
    }
}
