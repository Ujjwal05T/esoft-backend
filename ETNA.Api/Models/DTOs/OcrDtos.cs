namespace ETNA.Api.Models.DTOs;

// ==================== OCR REQUEST/RESPONSE DTOs ====================

/// <summary>
/// Request to scan an image for vehicle details
/// </summary>
public record OcrScanRequest(
    string Base64Image,
    string Mode // "plate" or "rc"
);

/// <summary>
/// Extracted vehicle data from OCR
/// </summary>
public record VehicleDataDto(
    string? PlateNumber,
    string? OwnerName,
    string? VehicleBrand,
    string? VehicleModel,
    int? Year,
    string? Variant,
    string? ChassisNumber,
    string? EngineNumber,
    string? FuelType,
    string? RegistrationDate,
    bool Success,
    string? ErrorMessage
);
