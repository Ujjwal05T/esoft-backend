using System.ComponentModel.DataAnnotations;

namespace ETNA.Api.Models.DTOs;

/// <summary>
/// DTO for login request
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for login response with JWT token
/// </summary>
public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfoDto? User { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// DTO for basic user info returned after login
/// </summary>
public class UserInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? WorkshopName { get; set; }
    public string? City { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
}
