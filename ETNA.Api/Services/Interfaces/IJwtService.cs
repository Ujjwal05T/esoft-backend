using ETNA.Api.Models.Entities;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for JWT token generation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate a JWT token for the given user
    /// </summary>
    string GenerateToken(WorkshopOwner owner, string role);
    
    /// <summary>
    /// Get token expiry time
    /// </summary>
    DateTime GetTokenExpiry();
}
