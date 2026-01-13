using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Service implementation for WorkshopStaff operations with SMS OTP
/// </summary>
public class WorkshopStaffService : IWorkshopStaffService
{
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IWorkshopOwnerRepository _ownerRepository;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<WorkshopStaffService> _logger;
    
    private const string PHONE_VERIFICATION_PURPOSE = "StaffPhoneVerification";
    
    public WorkshopStaffService(
        IWorkshopStaffRepository staffRepository,
        IWorkshopOwnerRepository ownerRepository,
        ISmsService smsService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<WorkshopStaffService> logger)
    {
        _staffRepository = staffRepository;
        _ownerRepository = ownerRepository;
        _smsService = smsService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }
    
    public async Task<ServiceResult<StaffResponseDto>> RegisterAsync(StaffRegisterDto dto)
    {
        try
        {
            // Check if email already exists
            var existingStaff = await _staffRepository.EmailExistsAsync(dto.Email.ToLower().Trim());
            if (existingStaff)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Email already registered");
            }
            
            // Check if workshop exists and is active
            var workshop = await _ownerRepository.GetByIdAsync(dto.WorkshopOwnerId);
            if (workshop == null)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Workshop not found");
            }
            
            if (!workshop.IsActive)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Workshop is not active");
            }
            
            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            // Create staff entity
            var staff = new WorkshopStaff
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                City = dto.City.Trim(),
                PasswordHash = passwordHash,
                WorkshopOwnerId = dto.WorkshopOwnerId,
                IsPhoneVerified = false,
                IsActive = false,
                RegistrationStatus = StaffRegistrationStatus.PendingPhoneVerification,
                CreatedAt = DateTime.UtcNow
            };
            
            // Save to database
            var staffId = await _staffRepository.CreateAsync(staff);
            staff.Id = staffId;
            
            // Generate and send OTP via EMAIL (temporarily, until SMS is configured)
            var otp = await _otpService.GenerateOtpAsync(dto.Email, PHONE_VERIFICATION_PURPOSE);
            await _emailService.SendOtpAsync(dto.Email, otp);
            
            _logger.LogInformation("Staff registered: {Email}, Phone: {Phone}, ID: {Id}, OTP sent to email", 
                dto.Email, dto.PhoneNumber, staffId);
            
            return ServiceResult<StaffResponseDto>.SuccessResult(MapToResponseDto(staff, workshop.WorkshopName), 
                "Registration successful! Please check your email for OTP.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering staff: {Email}", dto.Email);
            return ServiceResult<StaffResponseDto>.FailureResult("An error occurred during registration");
        }
    }
    
    public async Task<ServiceResult<StaffResponseDto>> VerifyPhoneAsync(StaffVerifyPhoneDto dto)
    {
        try
        {
            // Use email to find staff (temporarily, until SMS is configured)
            var staff = await _staffRepository.GetByEmailAsync(dto.PhoneNumber.Trim());
            
            if (staff == null)
            {
                // Try by phone number as fallback
                staff = await _staffRepository.GetByPhoneNumberAsync(dto.PhoneNumber.Trim());
            }
            
            if (staff == null)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Staff not found");
            }
            
            if (staff.IsPhoneVerified)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Already verified");
            }
            
            // Validate OTP using email as key
            var isValid = await _otpService.ValidateOtpAsync(staff.Email, dto.Otp, PHONE_VERIFICATION_PURPOSE);
            
            if (!isValid)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Invalid or expired OTP");
            }
            
            // Invalidate OTP
            await _otpService.InvalidateOtpAsync(staff.Email, PHONE_VERIFICATION_PURPOSE);
            
            // Update verification status - move to pending owner approval
            await _staffRepository.UpdatePhoneVerificationAsync(
                staff.Id, 
                true, 
                StaffRegistrationStatus.PendingOwnerApproval);
            
            staff.IsPhoneVerified = true;
            staff.RegistrationStatus = StaffRegistrationStatus.PendingOwnerApproval;
            
            // Get workshop name
            var workshop = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            
            // Notify workshop owner about new request via email
            if (workshop != null)
            {
                await _emailService.SendEmailAsync(
                    workshop.Email,
                    "New Staff Registration Request",
                    $"<h2>New Staff Request</h2>" +
                    $"<p><strong>{staff.Name}</strong> has requested to join your workshop <strong>{workshop.WorkshopName}</strong>.</p>" +
                    $"<p>Email: {staff.Email}</p>" +
                    $"<p>Phone: {staff.PhoneNumber}</p>" +
                    $"<p>Please login to your dashboard to approve or reject this request.</p>"
                );
            }
            
            _logger.LogInformation("Staff phone verified: {Phone}", dto.PhoneNumber);
            
            return ServiceResult<StaffResponseDto>.SuccessResult(
                MapToResponseDto(staff, workshop?.WorkshopName), 
                "Phone verified! Waiting for workshop owner approval.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying staff phone: {Phone}", dto.PhoneNumber);
            return ServiceResult<StaffResponseDto>.FailureResult("An error occurred during verification");
        }
    }
    
    public async Task<ServiceResult<bool>> ResendOtpAsync(string phoneNumber)
    {
        try
        {
            // Try finding by email first, then phone
            var staff = await _staffRepository.GetByEmailAsync(phoneNumber.Trim());
            if (staff == null)
            {
                staff = await _staffRepository.GetByPhoneNumberAsync(phoneNumber.Trim());
            }
            
            if (staff == null)
            {
                return ServiceResult<bool>.FailureResult("Staff not found");
            }
            
            if (staff.IsPhoneVerified)
            {
                return ServiceResult<bool>.FailureResult("Already verified");
            }
            
            // Generate and send new OTP via EMAIL
            var otp = await _otpService.GenerateOtpAsync(staff.Email, PHONE_VERIFICATION_PURPOSE);
            await _emailService.SendOtpAsync(staff.Email, otp);
            
            _logger.LogInformation("OTP resent to email: {Email}", staff.Email);
            
            return ServiceResult<bool>.SuccessResult(true, "OTP sent to your email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP: {Input}", phoneNumber);
            return ServiceResult<bool>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<StaffResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            
            if (staff == null)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Staff not found");
            }
            
            var workshop = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            
            return ServiceResult<StaffResponseDto>.SuccessResult(MapToResponseDto(staff, workshop?.WorkshopName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff by ID: {Id}", id);
            return ServiceResult<StaffResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<StaffResponseDto>> GetByEmailAsync(string email)
    {
        try
        {
            var staff = await _staffRepository.GetByEmailAsync(email.ToLower().Trim());
            
            if (staff == null)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Staff not found");
            }
            
            var workshop = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            
            return ServiceResult<StaffResponseDto>.SuccessResult(MapToResponseDto(staff, workshop?.WorkshopName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff by email: {Email}", email);
            return ServiceResult<StaffResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<IEnumerable<WorkshopListItemDto>>> GetWorkshopsByCityAsync(string city)
    {
        try
        {
            var workshops = await _ownerRepository.GetByCityAsync(city.Trim());
            
            var result = workshops.Select(w => new WorkshopListItemDto
            {
                Id = w.Id,
                WorkshopName = w.WorkshopName,
                OwnerName = w.OwnerName,
                Address = w.Address,
                City = w.City
            });
            
            return ServiceResult<IEnumerable<WorkshopListItemDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workshops by city: {City}", city);
            return ServiceResult<IEnumerable<WorkshopListItemDto>>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<IEnumerable<PendingStaffRequestDto>>> GetPendingRequestsAsync(int workshopOwnerId)
    {
        try
        {
            var pendingStaff = await _staffRepository.GetPendingApprovalsByWorkshopOwnerIdAsync(workshopOwnerId);
            
            var result = pendingStaff.Select(s => new PendingStaffRequestDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                City = s.City,
                RequestedAt = s.CreatedAt
            });
            
            return ServiceResult<IEnumerable<PendingStaffRequestDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending requests for owner: {OwnerId}", workshopOwnerId);
            return ServiceResult<IEnumerable<PendingStaffRequestDto>>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<StaffResponseDto>> ProcessApprovalAsync(StaffApprovalDto dto, int approverOwnerId)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(dto.StaffId);
            
            if (staff == null)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Staff not found");
            }
            
            if (staff.RegistrationStatus != StaffRegistrationStatus.PendingOwnerApproval)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("Staff is not pending approval");
            }
            
            // Verify the approver is the workshop owner
            if (staff.WorkshopOwnerId != approverOwnerId)
            {
                return ServiceResult<StaffResponseDto>.FailureResult("You are not authorized to approve this request");
            }
            
            var newStatus = dto.Approve 
                ? StaffRegistrationStatus.Approved 
                : StaffRegistrationStatus.Rejected;
            
            await _staffRepository.UpdateApprovalStatusAsync(
                dto.StaffId,
                newStatus,
                approverOwnerId,
                dto.RejectionReason);
            
            staff.RegistrationStatus = newStatus;
            staff.IsActive = dto.Approve;
            staff.ApprovedByOwnerId = approverOwnerId;
            staff.ApprovedAt = dto.Approve ? DateTime.UtcNow : null;
            staff.RejectionReason = dto.RejectionReason;
            
            var workshop = await _ownerRepository.GetByIdAsync(staff.WorkshopOwnerId);
            
            // Notify staff about the decision via email
            var subject = dto.Approve ? "Registration Approved!" : "Registration Update";
            var body = dto.Approve
                ? $"<h2>Congratulations!</h2><p>Your registration request to join <strong>{workshop?.WorkshopName}</strong> has been approved.</p><p>You can now login to access your dashboard.</p>"
                : $"<h2>Registration Update</h2><p>Your registration request to join <strong>{workshop?.WorkshopName}</strong> has been declined.</p>" +
                  (!string.IsNullOrEmpty(dto.RejectionReason) ? $"<p>Reason: {dto.RejectionReason}</p>" : "");
            
            await _emailService.SendEmailAsync(staff.Email, subject, body);
            
            // Also send SMS notification
            var smsMessage = dto.Approve
                ? $"ETNA: Your registration to {workshop?.WorkshopName} has been approved! Login to access your dashboard."
                : $"ETNA: Your registration request has been declined.";
            await _smsService.SendSmsAsync(staff.PhoneNumber, smsMessage);
            
            _logger.LogInformation("Staff {Action}: {Email} by Owner: {OwnerId}", 
                dto.Approve ? "approved" : "rejected", staff.Email, approverOwnerId);
            
            return ServiceResult<StaffResponseDto>.SuccessResult(
                MapToResponseDto(staff, workshop?.WorkshopName),
                dto.Approve ? "Staff approved successfully" : "Staff rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval for staff: {StaffId}", dto.StaffId);
            return ServiceResult<StaffResponseDto>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetStaffByWorkshopAsync(int workshopOwnerId)
    {
        try
        {
            var staff = await _staffRepository.GetByWorkshopOwnerIdAsync(workshopOwnerId);
            var workshop = await _ownerRepository.GetByIdAsync(workshopOwnerId);
            
            var result = staff.Select(s => MapToResponseDto(s, workshop?.WorkshopName));
            
            return ServiceResult<IEnumerable<StaffResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff for workshop: {OwnerId}", workshopOwnerId);
            return ServiceResult<IEnumerable<StaffResponseDto>>.FailureResult("An error occurred");
        }
    }
    
    public async Task<ServiceResult<StaffResponseDto>> CheckStatusAsync(string email)
    {
        return await GetByEmailAsync(email);
    }
    
    private static StaffResponseDto MapToResponseDto(WorkshopStaff staff, string? workshopName)
    {
        return new StaffResponseDto
        {
            Id = staff.Id,
            Name = staff.Name,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            City = staff.City,
            WorkshopOwnerId = staff.WorkshopOwnerId,
            WorkshopName = workshopName,
            PhotoUrl = staff.PhotoUrl,
            IsPhoneVerified = staff.IsPhoneVerified,
            IsActive = staff.IsActive,
            RegistrationStatus = staff.RegistrationStatus.ToString(),
            CreatedAt = staff.CreatedAt,
            ApprovedAt = staff.ApprovedAt
        };
    }
}
