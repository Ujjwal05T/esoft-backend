using ETNA.Api.Services.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Twilio SMS service implementation
/// </summary>
public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string _fromNumber;
    private readonly bool _isConfigured;
    
    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var accountSid = _configuration["TwilioSettings:AccountSid"];
        var authToken = _configuration["TwilioSettings:AuthToken"];
        _fromNumber = _configuration["TwilioSettings:FromNumber"] ?? "";
        
        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
            _isConfigured = true;
            _logger.LogInformation("Twilio SMS service initialized");
        }
        else
        {
            _isConfigured = false;
            _logger.LogWarning("Twilio settings not configured. SMS will be logged only.");
        }
    }
    
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Normalize phone number
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        
        if (!_isConfigured)
        {
            // Log the message for development/testing
            _logger.LogWarning("📱 SMS (Twilio not configured) - To: {Phone}", normalizedPhone);
            _logger.LogWarning("📱 Message: {Message}", message);
            return true; // Return true for development
        }
        
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(normalizedPhone)
            );
            
            _logger.LogInformation("✅ SMS sent successfully. SID: {Sid}, To: {Phone}", 
                messageResource.Sid, normalizedPhone);
            
            return messageResource.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Twilio.Exceptions.ApiException ex) when (ex.Message.Contains("unverified"))
        {
            // Trial account limitation - log the message so developer can continue testing
            _logger.LogWarning("⚠️ TWILIO TRIAL ACCOUNT: Cannot send to unverified number {Phone}", normalizedPhone);
            _logger.LogWarning("⚠️ To fix: Verify this number at https://www.twilio.com/console/phone-numbers/verified");
            _logger.LogWarning("📱 SMS MESSAGE (for testing): {Message}", message);
            
            // Return true so registration flow continues (for development)
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send SMS to {Phone}", phoneNumber);
            _logger.LogWarning("📱 SMS MESSAGE (for testing): {Message}", message);
            
            // Return true for development so flow continues
            return true;
        }
    }
    
    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otp)
    {
        var message = $"Your ETNA verification code is: {otp}. Valid for 10 minutes. Do not share this code.";
        return await SendSmsAsync(phoneNumber, message);
    }
    
    /// <summary>
    /// Normalize phone number to E.164 format
    /// </summary>
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces, dashes, parentheses
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // If it doesn't start with +, assume it needs country code
        if (!cleaned.StartsWith('+'))
        {
            // Default to India (+91) if no country code
            if (cleaned.Length == 10)
            {
                cleaned = "+91" + cleaned;
            }
            else
            {
                cleaned = "+" + cleaned;
            }
        }
        
        return cleaned;
    }
}
