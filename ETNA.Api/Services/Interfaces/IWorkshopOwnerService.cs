using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for WorkshopOwner business logic
/// </summary>
public interface IWorkshopOwnerService
{
    Task<ServiceResult<WorkshopOwnerResponseDto>> RegisterAsync(WorkshopOwnerRegisterDto dto);
    Task<ServiceResult<WorkshopOwnerResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<WorkshopOwnerResponseDto>> GetByEmailAsync(string email);
    Task<ServiceResult<bool>> VerifyEmailAsync(VerifyEmailDto dto);
    Task<ServiceResult<bool>> ResendOtpAsync(string email);
    Task<ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>> GetActiveWorkshopsAsync();
    Task<ServiceResult<IEnumerable<WorkshopOwnerResponseDto>>> GetWorkshopsByCityAsync(string city);
}

/// <summary>
/// Generic service result wrapper
/// </summary>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ServiceResult<T> SuccessResult(T data, string? message = null)
    {
        return new ServiceResult<T> { Success = true, Data = data, Message = message };
    }
    
    public static ServiceResult<T> FailureResult(string error)
    {
        return new ServiceResult<T> { Success = false, Errors = new List<string> { error } };
    }
    
    public static ServiceResult<T> FailureResult(List<string> errors)
    {
        return new ServiceResult<T> { Success = false, Errors = errors };
    }
}
