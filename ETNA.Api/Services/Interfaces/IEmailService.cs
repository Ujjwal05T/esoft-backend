namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for sending emails
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendOtpEmailAsync(string to, string otp, string purpose);
    Task<bool> SendOtpAsync(string to, string otp);
}
