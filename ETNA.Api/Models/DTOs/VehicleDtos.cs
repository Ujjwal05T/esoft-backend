namespace ETNA.Api.Models.DTOs;

// ==================== VEHICLE DTOs ====================

/// <summary>
/// Request to create a new vehicle
/// </summary>
public record CreateVehicleRequest(
    string PlateNumber,
    string? Brand,
    string? Model,
    int? Year,
    string? Variant,
    string? ChassisNumber,
    string? Specs,
    string? RegistrationName,
    string OwnerName,
    string ContactNumber,
    string? Email,
    string? GstNumber,
    string? InsuranceProvider,
    string? OdometerReading,
    string? Observations
);

/// <summary>
/// Request to update a vehicle
/// </summary>
public record UpdateVehicleRequest(
    string PlateNumber,
    string? Brand,
    string? Model,
    int? Year,
    string? Variant,
    string? ChassisNumber,
    string? Specs,
    string? RegistrationName,
    string OwnerName,
    string ContactNumber,
    string? Email,
    string? GstNumber,
    string? InsuranceProvider,
    string? OdometerReading,
    string? Observations,
    int Status
);

/// <summary>
/// Vehicle response DTO
/// </summary>
public record VehicleResponse(
    int Id,
    string PlateNumber,
    string? Brand,
    string? Model,
    int? Year,
    string? Variant,
    string? ChassisNumber,
    string? Specs,
    string? RegistrationName,
    string OwnerName,
    string ContactNumber,
    string? Email,
    string? GstNumber,
    string? InsuranceProvider,
    string? OdometerReading,
    string? Observations,
    string? ObservationsAudioUrl,
    int WorkshopOwnerId,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Vehicle list response with count
/// </summary>
public record VehicleListResponse(
    List<VehicleResponse> Vehicles,
    int TotalCount
);

