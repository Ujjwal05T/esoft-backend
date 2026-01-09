using System.Net;
using System.Net.Mail;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// SMTP Email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            
            var host = smtpSettings["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"] ?? throw new InvalidOperationException("SMTP Username not configured");
            var password = smtpSettings["Password"] ?? throw new InvalidOperationException("SMTP Password not configured");
            var fromEmail = smtpSettings["FromEmail"] ?? username;
            var fromName = smtpSettings["FromName"] ?? "ETNA";
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
            
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);
            
            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }
    
    public async Task<bool> SendOtpEmailAsync(string to, string otp, string purpose)
    {
        var subject = purpose switch
        {
            "Registration" => "ETNA - Verify Your Email",
            "PasswordReset" => "ETNA - Password Reset OTP",
            "ETNATeamVerification" => "ETNA - Team Verification Code",
            _ => "ETNA - Your OTP Code"
        };
        
        var body = GetOtpEmailTemplate(otp, purpose);
        
        return await SendEmailAsync(to, subject, body, true);
    }
    
    private static string GetOtpEmailTemplate(string otp, string purpose)
    {
        var purposeText = purpose switch
        {
            "Registration" => "verify your email address",
            "PasswordReset" => "reset your password",
            "ETNATeamVerification" => "complete ETNA team verification",
            _ => "complete your request"
        };
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 40px 0; text-align: center;'>
                <table role='presentation' style='width: 100%; max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='padding: 40px 40px 20px; text-align: center; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 8px 8px 0 0;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: bold;'>ETNA</h1>
                            <p style='margin: 10px 0 0; color: rgba(255,255,255,0.9); font-size: 14px;'>Vehicle Service Management</p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            <h2 style='margin: 0 0 20px; color: #333333; font-size: 24px; text-align: center;'>Your Verification Code</h2>
                            <p style='margin: 0 0 30px; color: #666666; font-size: 16px; line-height: 1.5; text-align: center;'>
                                Use the following OTP to {purposeText}. This code is valid for <strong>10 minutes</strong>.
                            </p>
                            
                            <!-- OTP Box -->
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px 40px; border-radius: 8px;'>
                                    <span style='font-size: 36px; font-weight: bold; color: #ffffff; letter-spacing: 8px;'>{otp}</span>
                                </div>
                            </div>
                            
                            <p style='margin: 30px 0 0; color: #999999; font-size: 14px; text-align: center;'>
                                If you didn't request this code, please ignore this email.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 20px 40px; background-color: #f8f9fa; border-radius: 0 0 8px 8px; text-align: center;'>
                            <p style='margin: 0; color: #999999; font-size: 12px;'>
                                © 2026 ETNA. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
