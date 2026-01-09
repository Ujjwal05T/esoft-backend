using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    
    /// <summary>
    /// Validate if user can login (email verified, not rejected, etc.)
    /// </summary>
    Task<ServiceResult<bool>> ValidateUserCanLoginAsync(string email);
}
