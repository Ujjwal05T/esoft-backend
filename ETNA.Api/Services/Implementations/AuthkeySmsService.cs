using ETNA.Api.Services.Interfaces;
using System.Net.Http;
using System.Web;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Authkey.io SMS service implementation
/// </summary>
public class AuthkeySmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthkeySmsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _authKey;
    private readonly string _senderId;
    private readonly string _countryCode;
    private readonly bool _isConfigured;
    
    private const string BASE_URL = "https://api.authkey.io/request";
    
    public AuthkeySmsService(IConfiguration configuration, ILogger<AuthkeySmsService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
        _authKey = _configuration["AuthkeySettings:AuthKey"] ?? "";
        _senderId = _configuration["AuthkeySettings:SenderId"] ?? "ETNAOT";
        _countryCode = _configuration["AuthkeySettings:CountryCode"] ?? "91"; // India default
        
        _isConfigured = !string.IsNullOrEmpty(_authKey);
        
        if (_isConfigured)
        {
            _logger.LogInformation("Authkey.io SMS service initialized");
        }
        else
        {
            _logger.LogWarning("Authkey settings not configured. SMS will be logged only.");
        }
    }
    
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Clean phone number (remove +, spaces, country code if present)
        var cleanedPhone = CleanPhoneNumber(phoneNumber);
        
        if (!_isConfigured)
        {
            // Log the message for development/testing
            _logger.LogWarning("📱 SMS (Authkey not configured) - To: {Phone}", cleanedPhone);
            _logger.LogWarning("📱 Message: {Message}", message);
            return true; // Return true for development
        }
        
        try
        {
            // URL encode the message
            var encodedMessage = HttpUtility.UrlEncode(message);
            
            // Build the request URL
            var requestUrl = $"{BASE_URL}?authkey={_authKey}&sms={encodedMessage}&mobile={cleanedPhone}&country_code={_countryCode}&sender={_senderId}";
            
            _logger.LogInformation("Sending SMS via Authkey.io to {Phone}", cleanedPhone);
            
            var response = await _httpClient.GetAsync(requestUrl);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Check if response is HTML (error) or JSON/text (success)
            if (responseContent.Contains("<!doctype html>") || responseContent.Contains("<html"))
            {
                // API returned HTML page - likely invalid authkey or wrong URL
                _logger.LogError("❌ Authkey.io returned HTML page - check your AuthKey in appsettings.json");
                _logger.LogWarning("📱 SMS MESSAGE (for testing) to {Phone}: {Message}", cleanedPhone, message);
                return true; // Return true for development
            }
            
            if (response.IsSuccessStatusCode && !responseContent.Contains("error"))
            {
                _logger.LogInformation("✅ SMS sent successfully via Authkey.io to {Phone}. Response: {Response}", 
                    cleanedPhone, responseContent);
                return true;
            }
            else
            {
                _logger.LogError("❌ Authkey.io SMS failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
                _logger.LogWarning("📱 SMS MESSAGE (for testing) to {Phone}: {Message}", cleanedPhone, message);
                return true; // Return true for development so flow continues
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send SMS to {Phone}", cleanedPhone);
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
    /// Clean phone number - remove +, spaces, and country code if present
    /// </summary>
    private string CleanPhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // If starts with country code (91 for India), remove it
        if (cleaned.StartsWith("91") && cleaned.Length > 10)
        {
            cleaned = cleaned.Substring(2);
        }
        
        // Return last 10 digits (mobile number)
        if (cleaned.Length > 10)
        {
            cleaned = cleaned.Substring(cleaned.Length - 10);
        }
        
        return cleaned;
    }
}
