using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ETNA.Api.Models.Entities;
using ETNA.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// JWT service implementation for token generation
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    
    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public string GenerateToken(WorkshopOwner owner, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "ETNA.Api";
        var audience = jwtSettings["Audience"] ?? "ETNA.Client";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "43200"); // 30 days default
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, owner.Id.ToString()),
            new(ClaimTypes.Email, owner.Email),
            new(ClaimTypes.Name, owner.OwnerName),
            new(ClaimTypes.Role, role),
            new("WorkshopName", owner.WorkshopName),
            new("City", owner.City),
            new("IsActive", owner.IsActive.ToString()),
            new("RegistrationStatus", owner.RegistrationStatus.ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("JWT token generated for user: {Email}, expires in {Days} days", owner.Email, expiryMinutes / 1440);
        
        return tokenString;
    }
    
    public DateTime GetTokenExpiry()
    {
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "43200");
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }
}
