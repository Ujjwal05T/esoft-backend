using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for ETNA verification and photo upload operations
/// </summary>
public interface IVerificationService
{
    /// <summary>
    /// Initiate ETNA verification - sends OTP to owner
    /// </summary>
    Task<ServiceResult<bool>> InitiateETNAVerificationAsync(InitiateETNAVerificationDto dto);
    
    /// <summary>
    /// Complete ETNA verification with both OTPs
    /// </summary>
    Task<ServiceResult<ETNAVerificationResponseDto>> CompleteETNAVerificationAsync(ETNAVerificationDto dto);
    
    /// <summary>
    /// Upload owner photo
    /// </summary>
    Task<ServiceResult<PhotoUploadResponseDto>> UploadOwnerPhotoAsync(string email, Stream fileStream, string fileName);
    
    /// <summary>
    /// Upload workshop photo
    /// </summary>
    Task<ServiceResult<PhotoUploadResponseDto>> UploadWorkshopPhotoAsync(string email, Stream fileStream, string fileName);
    
    /// <summary>
    /// Complete photo upload and activate account
    /// </summary>
    Task<ServiceResult<PhotoUploadResponseDto>> CompletePhotoUploadAsync(string email);
}
