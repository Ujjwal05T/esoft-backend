using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Service implementation for ETNA verification and photo upload
/// </summary>
public class VerificationService : IVerificationService
{
    private readonly IWorkshopOwnerRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<VerificationService> _logger;
    
    // Hardcoded ETNA team OTP
    private const string ETNA_TEAM_OTP = "111111";
    private const string ETNA_VERIFICATION_PURPOSE = "ETNAVerification";
    
    public VerificationService(
        IWorkshopOwnerRepository repository,
        IEmailService emailService,
        IOtpService otpService,
        IFileUploadService fileUploadService,
        ILogger<VerificationService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _otpService = otpService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }
    
    public async Task<ServiceResult<bool>> InitiateETNAVerificationAsync(InitiateETNAVerificationDto dto)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(dto.OwnerEmail.ToLower().Trim());
            
            if (owner == null)
            {
                return ServiceResult<bool>.FailureResult("Workshop owner not found");
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.PendingETNAVerification)
            {
                return ServiceResult<bool>.FailureResult($"Invalid status for ETNA verification. Current status: {owner.RegistrationStatus}");
            }
            
            // Generate and send OTP to owner's email
            var otp = await _otpService.GenerateOtpAsync(owner.Email, ETNA_VERIFICATION_PURPOSE);
            var emailSent = await _emailService.SendOtpEmailAsync(owner.Email, otp, ETNA_VERIFICATION_PURPOSE);
            
            if (!emailSent)
            {
                return ServiceResult<bool>.FailureResult("Failed to send OTP email to owner");
            }
            
            _logger.LogInformation("ETNA verification initiated for {Email} by {ETNAMember}", 
                dto.OwnerEmail, dto.ETNAMemberName);
            
            return ServiceResult<bool>.SuccessResult(true, 
                $"OTP sent to owner's email. ETNA team OTP is: {ETNA_TEAM_OTP} (for development)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating ETNA verification for {Email}", dto.OwnerEmail);
            return ServiceResult<bool>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<ETNAVerificationResponseDto>> CompleteETNAVerificationAsync(ETNAVerificationDto dto)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(dto.OwnerEmail.ToLower().Trim());
            
            if (owner == null)
            {
                return ServiceResult<ETNAVerificationResponseDto>.FailureResult("Workshop owner not found");
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.PendingETNAVerification)
            {
                return ServiceResult<ETNAVerificationResponseDto>.FailureResult(
                    $"Invalid status for ETNA verification. Current status: {owner.RegistrationStatus}");
            }
            
            // Validate ETNA team OTP (hardcoded)
            if (dto.ETNAOtp != ETNA_TEAM_OTP)
            {
                return ServiceResult<ETNAVerificationResponseDto>.FailureResult("Invalid ETNA team OTP");
            }
            
            // Validate owner's email OTP
            var isOwnerOtpValid = await _otpService.ValidateOtpAsync(
                dto.OwnerEmail, dto.OwnerOtp, ETNA_VERIFICATION_PURPOSE);
            
            if (!isOwnerOtpValid)
            {
                return ServiceResult<ETNAVerificationResponseDto>.FailureResult("Invalid or expired owner OTP");
            }
            
            // Invalidate OTP after successful verification
            await _otpService.InvalidateOtpAsync(dto.OwnerEmail, ETNA_VERIFICATION_PURPOSE);
            
            // Update owner with ETNA verification details
            owner.ETNAVerifierName = dto.ETNAMemberName;
            owner.ETNAVerifierPhone = dto.ETNAMemberPhone;
            owner.IsETNAVerified = true;
            owner.ETNAVerifiedAt = DateTime.UtcNow;
            owner.RegistrationStatus = WorkshopOwnerRegistrationStatus.PendingPhotoUpload;
            owner.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(owner);
            
            _logger.LogInformation("ETNA verification completed for {Email}", dto.OwnerEmail);
            
            return ServiceResult<ETNAVerificationResponseDto>.SuccessResult(new ETNAVerificationResponseDto
            {
                Success = true,
                Message = "ETNA verification completed successfully",
                NextStep = "Please upload owner and workshop photos",
                RegistrationStatus = owner.RegistrationStatus.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing ETNA verification for {Email}", dto.OwnerEmail);
            return ServiceResult<ETNAVerificationResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<PhotoUploadResponseDto>> UploadOwnerPhotoAsync(
        string email, Stream fileStream, string fileName)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(email.ToLower().Trim());
            
            if (owner == null)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult("Workshop owner not found");
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.PendingPhotoUpload)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult(
                    $"Invalid status for photo upload. Current status: {owner.RegistrationStatus}");
            }
            
            if (!_fileUploadService.IsValidImageFile(fileName))
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult(
                    "Invalid file type. Allowed: jpg, jpeg, png, gif, webp");
            }
            
            // Upload owner photo
            var photoUrl = await _fileUploadService.UploadFileAsync(fileStream, fileName, "owners");
            
            // Update owner record
            owner.OwnerPhotoUrl = photoUrl;
            owner.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(owner);
            
            _logger.LogInformation("Owner photo uploaded for {Email}", email);
            
            return ServiceResult<PhotoUploadResponseDto>.SuccessResult(new PhotoUploadResponseDto
            {
                Success = true,
                Message = "Owner photo uploaded successfully",
                OwnerPhotoUrl = photoUrl,
                WorkshopPhotoUrl = owner.WorkshopPhotoUrl,
                RegistrationStatus = owner.RegistrationStatus.ToString(),
                IsActive = owner.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading owner photo for {Email}", email);
            return ServiceResult<PhotoUploadResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<PhotoUploadResponseDto>> UploadWorkshopPhotoAsync(
        string email, Stream fileStream, string fileName)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(email.ToLower().Trim());
            
            if (owner == null)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult("Workshop owner not found");
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.PendingPhotoUpload)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult(
                    $"Invalid status for photo upload. Current status: {owner.RegistrationStatus}");
            }
            
            if (!_fileUploadService.IsValidImageFile(fileName))
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult(
                    "Invalid file type. Allowed: jpg, jpeg, png, gif, webp");
            }
            
            // Upload workshop photo
            var photoUrl = await _fileUploadService.UploadFileAsync(fileStream, fileName, "workshops");
            
            // Update owner record
            owner.WorkshopPhotoUrl = photoUrl;
            owner.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(owner);
            
            _logger.LogInformation("Workshop photo uploaded for {Email}", email);
            
            return ServiceResult<PhotoUploadResponseDto>.SuccessResult(new PhotoUploadResponseDto
            {
                Success = true,
                Message = "Workshop photo uploaded successfully",
                OwnerPhotoUrl = owner.OwnerPhotoUrl,
                WorkshopPhotoUrl = photoUrl,
                RegistrationStatus = owner.RegistrationStatus.ToString(),
                IsActive = owner.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading workshop photo for {Email}", email);
            return ServiceResult<PhotoUploadResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<PhotoUploadResponseDto>> CompletePhotoUploadAsync(string email)
    {
        try
        {
            var owner = await _repository.GetByEmailAsync(email.ToLower().Trim());
            
            if (owner == null)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult("Workshop owner not found");
            }
            
            if (owner.RegistrationStatus != WorkshopOwnerRegistrationStatus.PendingPhotoUpload)
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult(
                    $"Invalid status. Current status: {owner.RegistrationStatus}");
            }
            
            // Check if both photos are uploaded
            if (string.IsNullOrEmpty(owner.OwnerPhotoUrl))
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult("Owner photo is required");
            }
            
            if (string.IsNullOrEmpty(owner.WorkshopPhotoUrl))
            {
                return ServiceResult<PhotoUploadResponseDto>.FailureResult("Workshop photo is required");
            }
            
            // Activate account
            owner.RegistrationStatus = WorkshopOwnerRegistrationStatus.Active;
            owner.IsActive = true;
            owner.ActivatedAt = DateTime.UtcNow;
            owner.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(owner);
            
            _logger.LogInformation("Account activated for {Email}", email);
            
            return ServiceResult<PhotoUploadResponseDto>.SuccessResult(new PhotoUploadResponseDto
            {
                Success = true,
                Message = "Registration completed! Your account is now active.",
                OwnerPhotoUrl = owner.OwnerPhotoUrl,
                WorkshopPhotoUrl = owner.WorkshopPhotoUrl,
                RegistrationStatus = owner.RegistrationStatus.ToString(),
                IsActive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing photo upload for {Email}", email);
            return ServiceResult<PhotoUploadResponseDto>.FailureResult("An error occurred");
        }
    }
}
