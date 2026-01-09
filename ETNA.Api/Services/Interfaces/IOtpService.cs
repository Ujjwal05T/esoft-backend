namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for OTP generation and validation
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generate and store a new OTP for the given email
    /// </summary>
    Task<string> GenerateOtpAsync(string email, string purpose);
    
    /// <summary>
    /// Validate the OTP for the given email
    /// </summary>
    Task<bool> ValidateOtpAsync(string email, string otp, string purpose);
    
    /// <summary>
    /// Invalidate/delete OTP after successful verification
    /// </summary>
    Task InvalidateOtpAsync(string email, string purpose);
}
