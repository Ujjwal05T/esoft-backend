namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for SMS operations
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send an SMS message
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    
    /// <summary>
    /// Send OTP via SMS
    /// </summary>
    Task<bool> SendOtpSmsAsync(string phoneNumber, string otp);
}
