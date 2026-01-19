using System.Security.Cryptography;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Service implementation for workshop registration and onboarding
/// </summary>
public class WorkshopRegistrationService : IWorkshopRegistrationService
{
    private readonly IWorkshopOwnerRepository _repository;
    private readonly IEmailService _emailService;
    
    // Hardcoded OTP for testing - will be replaced with actual SMS service later
    private const string TEST_OTP = "111111";
    
    // In-memory store for verified phone numbers (in production, use Redis or database)
    private static readonly Dictionary<string, DateTime> VerifiedPhones = new();
    private static readonly Dictionary<string, DateTime> VerifiedEtnaPhones = new();

    public WorkshopRegistrationService(
        IWorkshopOwnerRepository repository,
        IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    // ==================== Frontend (Mobile App) ====================

    public async Task<OtpSentResponse> SendRegistrationOtpAsync(string phoneNumber)
    {
        // Check if phone number already registered with active status
        var existing = await _repository.GetByPhoneNumberAsync(phoneNumber);
        if (existing != null && existing.RegistrationStatus == RegistrationStatus.Active)
        {
            return new OtpSentResponse(false, "Phone number already registered. Please login.", 0);
        }

        // TODO: Send actual OTP via SMS service
        // For now, use hardcoded OTP "111111"
        
        return new OtpSentResponse(true, "OTP sent successfully. Use 111111 for testing.", 300);
    }

    public async Task<OtpVerifiedResponse> VerifyRegistrationOtpAsync(string phoneNumber, string otp)
    {
        // Verify OTP (hardcoded for testing)
        if (otp != TEST_OTP)
        {
            return new OtpVerifiedResponse(false, "Invalid OTP. Please try again.", null);
        }

        // Mark phone as verified (store in memory for now)
        VerifiedPhones[phoneNumber] = DateTime.UtcNow.AddMinutes(30);

        return new OtpVerifiedResponse(true, "Phone number verified successfully.", phoneNumber);
    }

    public async Task<RegistrationSubmittedResponse> SubmitRegistrationAsync(WorkshopRegistrationRequest request)
    {
        // Check if phone was verified
        if (!VerifiedPhones.TryGetValue(request.PhoneNumber, out var expiry) || expiry < DateTime.UtcNow)
        {
            return new RegistrationSubmittedResponse(false, "Phone number not verified. Please verify OTP first.", 0, "error");
        }

        // Check if already exists
        var existing = await _repository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (existing != null)
        {
            if (existing.RegistrationStatus == RegistrationStatus.Active)
            {
                return new RegistrationSubmittedResponse(false, "Phone number already registered.", 0, "error");
            }
            
            // Update existing pending request
            existing.OwnerName = request.OwnerName;
            existing.Email = request.Email;
            existing.AadhaarNumber = request.AadhaarNumber;
            existing.WorkshopName = request.WorkshopName;
            existing.Address = request.Address;
            existing.Landmark = request.Landmark;
            existing.PinCode = request.PinCode;
            existing.City = request.City;
            existing.ContactPersonName = request.ContactPersonName;
            existing.ContactPersonMobile = request.ContactPersonMobile;
            existing.GSTNumber = request.GSTNumber;
            existing.IsPhoneVerified = true;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(existing);
            
            // Remove from verified phones
            VerifiedPhones.Remove(request.PhoneNumber);
            
            return new RegistrationSubmittedResponse(true, "Workshop registration updated successfully.", existing.Id, "pending");
        }

        // Create new workshop owner
        var workshopOwner = new WorkshopOwner
        {
            OwnerName = request.OwnerName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            AadhaarNumber = request.AadhaarNumber,
            WorkshopName = request.WorkshopName,
            Address = request.Address,
            Landmark = request.Landmark,
            PinCode = request.PinCode,
            City = request.City,
            ContactPersonName = request.ContactPersonName,
            ContactPersonMobile = request.ContactPersonMobile,
            GSTNumber = request.GSTNumber,
            Source = "app",
            IsPhoneVerified = true,
            RegistrationStatus = RegistrationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(workshopOwner);
        
        // Remove from verified phones
        VerifiedPhones.Remove(request.PhoneNumber);

        return new RegistrationSubmittedResponse(true, "Workshop registration submitted successfully. Our team will contact you soon.", id, "pending");
    }

    // ==================== Frontend-Portal (Admin) ====================

    public async Task<WorkshopRequestsResponse> GetPendingRequestsAsync(int page, int pageSize, string? source = null)
    {
        var requests = await _repository.GetPendingRequestsAsync(page, pageSize);
        
        // Filter by source if specified
        if (!string.IsNullOrEmpty(source))
        {
            requests = requests.Where(r => r.Source.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var requestDtos = requests.Select(r => new WorkshopRequestDto(
            r.Id,
            r.WorkshopName,
            r.OwnerName,
            r.PhoneNumber,
            r.Email,
            r.AadhaarNumber,
            r.Address,
            r.Landmark,
            r.PinCode,
            r.City,
            r.ContactPersonName,
            r.ContactPersonMobile,
            r.GSTNumber,
            r.Source,
            r.RegistrationStatus.ToString(),
            r.IsPhoneVerified,
            r.IsETNAVerified,
            r.CreatedAt,
            (int)(DateTime.UtcNow - r.CreatedAt).TotalDays
        )).ToList();

        var stats = new WorkshopRequestStats(
            await _repository.GetCountByStatusAsync(RegistrationStatus.Pending) + await _repository.GetCountByStatusAsync(RegistrationStatus.UnderReview),
            await _repository.GetCountBySourceAsync("whatsapp"),
            await _repository.GetCountBySourceAsync("phone_call"),
            await _repository.GetCountBySourceAsync("app")
        );

        var totalCount = await _repository.GetCountByStatusAsync(RegistrationStatus.Pending) + await _repository.GetCountByStatusAsync(RegistrationStatus.UnderReview);

        return new WorkshopRequestsResponse(requestDtos, stats, totalCount, page, pageSize);
    }

    public async Task<List<WorkshopDto>> GetOnboardedWorkshopsAsync()
    {
        var workshops = await _repository.GetByStatusAsync(RegistrationStatus.Active);
        
        return workshops.Select(w => new WorkshopDto(
            w.Id,
            w.WorkshopName,
            w.OwnerName,
            w.Email,
            w.PhoneNumber,
            w.City,
            w.RegistrationStatus.ToString(),
            w.CreatedAt,
            w.ActivatedAt
        )).ToList();
    }

    public async Task<WorkshopOwner?> GetWorkshopByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<OtpSentResponse> SendEtnaVerificationOtpAsync(int workshopId, string etnaMobileNumber)
    {
        var workshop = await _repository.GetByIdAsync(workshopId);
        if (workshop == null)
        {
            return new OtpSentResponse(false, "Workshop not found.", 0);
        }

        if (workshop.RegistrationStatus == RegistrationStatus.Active)
        {
            return new OtpSentResponse(false, "Workshop is already onboarded.", 0);
        }

        // TODO: Send actual OTP via SMS service
        // For now, use hardcoded OTP "111111"
        
        // Store verification attempt
        var key = $"{workshopId}_{etnaMobileNumber}";
        VerifiedEtnaPhones[key] = DateTime.UtcNow.AddMinutes(10);

        return new OtpSentResponse(true, "OTP sent to ETNA verifier. Use 111111 for testing.", 300);
    }

    public async Task<OtpVerifiedResponse> VerifyEtnaOtpAsync(int workshopId, string etnaMobileNumber, string otp)
    {
        // Verify OTP (hardcoded for testing)
        if (otp != TEST_OTP)
        {
            return new OtpVerifiedResponse(false, "Invalid OTP. Please try again.", null);
        }

        var workshop = await _repository.GetByIdAsync(workshopId);
        if (workshop == null)
        {
            return new OtpVerifiedResponse(false, "Workshop not found.", null);
        }

        // Mark as ETNA verified
        await _repository.UpdateETNAVerificationAsync(workshopId, "ETNA Team", etnaMobileNumber);
        await _repository.UpdateStatusAsync(workshopId, RegistrationStatus.UnderReview);

        // Store verification for onboarding
        var key = $"{workshopId}_{etnaMobileNumber}";
        VerifiedEtnaPhones[key] = DateTime.UtcNow.AddMinutes(30);

        return new OtpVerifiedResponse(true, "ETNA verification successful.", workshopId.ToString());
    }

    public async Task<ApiResponse> UploadDocumentsAsync(int workshopId, string? tradeLicenseUrl, string? workshopPhotoUrl, string? ownerPhotoUrl)
    {
        var workshop = await _repository.GetByIdAsync(workshopId);
        if (workshop == null)
        {
            return new ApiResponse(false, "Workshop not found.");
        }

        await _repository.UpdateDocumentsAsync(workshopId, tradeLicenseUrl, workshopPhotoUrl, ownerPhotoUrl);

        return new ApiResponse(true, "Documents uploaded successfully.");
    }

    public async Task<OnboardingCompletedResponse> CompleteOnboardingAsync(int workshopId, CompleteOnboardingRequest request)
    {
        var workshop = await _repository.GetByIdAsync(workshopId);
        if (workshop == null)
        {
            return new OnboardingCompletedResponse(false, "Workshop not found.", 0, "", null, false);
        }

        if (workshop.RegistrationStatus == RegistrationStatus.Active)
        {
            return new OnboardingCompletedResponse(false, "Workshop is already onboarded.", workshopId, workshop.WorkshopName, workshop.Email, false);
        }

        // Update ETNA verifier name if provided
        if (!string.IsNullOrEmpty(request.EtnaVerifierName))
        {
            workshop.ETNAVerifierName = request.EtnaVerifierName;
            await _repository.UpdateAsync(workshop);
        }

        // Generate password if requested
        string? generatedPassword = null;
        if (request.GeneratePassword)
        {
            generatedPassword = GenerateRandomPassword();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword);
            await _repository.ActivateWorkshopAsync(workshopId, passwordHash);
        }
        else
        {
            await _repository.UpdateStatusAsync(workshopId, RegistrationStatus.Active);
        }

        // Send credentials via email if requested
        bool credentialsSent = false;
        if (request.SendCredentials && !string.IsNullOrEmpty(workshop.Email) && !string.IsNullOrEmpty(generatedPassword))
        {
            try
            {
                await _emailService.SendEmailAsync(
                    workshop.Email,
                    "Welcome to ETNA - Your Workshop Credentials",
                    $@"
                    <h2>Welcome to ETNA!</h2>
                    <p>Dear {workshop.OwnerName},</p>
                    <p>Your workshop <strong>{workshop.WorkshopName}</strong> has been successfully onboarded.</p>
                    <p><strong>Login Credentials:</strong></p>
                    <ul>
                        <li>Email: {workshop.Email}</li>
                        <li>Password: {generatedPassword}</li>
                    </ul>
                    <p>Please change your password after first login.</p>
                    <p>Thank you for joining ETNA!</p>
                    "
                );
                credentialsSent = true;
            }
            catch
            {
                // Email failed, but onboarding is complete
                credentialsSent = false;
            }
        }

        return new OnboardingCompletedResponse(
            true,
            "Workshop onboarded successfully.",
            workshopId,
            workshop.WorkshopName,
            workshop.Email,
            credentialsSent
        );
    }

    public async Task<ApiResponse> RejectWorkshopAsync(int workshopId, string reason)
    {
        var workshop = await _repository.GetByIdAsync(workshopId);
        if (workshop == null)
        {
            return new ApiResponse(false, "Workshop not found.");
        }

        await _repository.UpdateStatusAsync(workshopId, RegistrationStatus.Rejected);

        // TODO: Optionally send rejection email/SMS

        return new ApiResponse(true, $"Workshop request rejected. Reason: {reason}");
    }

    private static string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        var random = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        return new string(random.Select(b => chars[b % chars.Length]).ToArray());
    }
}
