using System.Collections.Concurrent;
using System.Security.Cryptography;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// In-memory OTP service implementation
/// For production, consider using Redis or database storage
/// </summary>
public class OtpService : IOtpService
{
    private readonly ILogger<OtpService> _logger;
    
    // In-memory storage for OTPs (key: email_purpose, value: OtpRecord)
    // For production, use Redis or database
    private static readonly ConcurrentDictionary<string, OtpRecord> _otpStore = new();
    
    // OTP configuration
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 10;
    private const int MaxAttempts = 5;
    
    public OtpService(ILogger<OtpService> logger)
    {
        _logger = logger;
    }
    
    public Task<string> GenerateOtpAsync(string email, string purpose)
    {
        var key = GetKey(email, purpose);
        var otp = GenerateRandomOtp();
        
        var record = new OtpRecord
        {
            Otp = otp,
            Email = email.ToLower().Trim(),
            Purpose = purpose,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            Attempts = 0
        };
        
        // Store or update OTP
        _otpStore.AddOrUpdate(key, record, (_, _) => record);
        
        _logger.LogInformation("OTP generated for {Email}, purpose: {Purpose}", email, purpose);
        
        return Task.FromResult(otp);
    }
    
    public Task<bool> ValidateOtpAsync(string email, string otp, string purpose)
    {
        var key = GetKey(email, purpose);
        
        if (!_otpStore.TryGetValue(key, out var record))
        {
            _logger.LogWarning("OTP not found for {Email}, purpose: {Purpose}", email, purpose);
            return Task.FromResult(false);
        }
        
        // Check if expired
        if (DateTime.UtcNow > record.ExpiresAt)
        {
            _logger.LogWarning("OTP expired for {Email}, purpose: {Purpose}", email, purpose);
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(false);
        }
        
        // Check max attempts
        if (record.Attempts >= MaxAttempts)
        {
            _logger.LogWarning("Max OTP attempts exceeded for {Email}, purpose: {Purpose}", email, purpose);
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(false);
        }
        
        // Increment attempts
        record.Attempts++;
        
        // Validate OTP
        if (record.Otp != otp)
        {
            _logger.LogWarning("Invalid OTP attempt for {Email}, purpose: {Purpose}, attempt: {Attempt}", 
                email, purpose, record.Attempts);
            return Task.FromResult(false);
        }
        
        _logger.LogInformation("OTP validated successfully for {Email}, purpose: {Purpose}", email, purpose);
        return Task.FromResult(true);
    }
    
    public Task InvalidateOtpAsync(string email, string purpose)
    {
        var key = GetKey(email, purpose);
        _otpStore.TryRemove(key, out _);
        
        _logger.LogInformation("OTP invalidated for {Email}, purpose: {Purpose}", email, purpose);
        return Task.CompletedTask;
    }
    
    private static string GetKey(string email, string purpose)
    {
        return $"{email.ToLower().Trim()}_{purpose}";
    }
    
    private static string GenerateRandomOtp()
    {
        // Generate cryptographically secure random OTP
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return number.ToString().PadLeft(OtpLength, '0');
    }
    
    private class OtpRecord
    {
        public string Otp { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
    }
}
