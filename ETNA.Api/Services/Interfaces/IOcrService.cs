using ETNA.Api.Models.DTOs;

namespace ETNA.Api.Services.Interfaces;

public interface IOcrService
{
    /// <summary>
    /// Processes an image to extract vehicle details
    /// </summary>
    /// <param name="base64Image">The base64 encoded image string (with or without data URL prefix)</param>
    /// <param name="mode">The scanning mode: "plate" for number plate, "rc" for RC card</param>
    /// <returns>Extracted vehicle data</returns>
    Task<VehicleDataDto> ProcessImageAsync(string base64Image, string mode);
}
