namespace ETNA.Api.Models.DTOs;

// ==================== VEHICLE VISIT DTOs ====================

/// <summary>
/// Request to create a new vehicle visit (Gate In)
/// </summary>
public record CreateVehicleVisitRequest(
    int VehicleId,
    DateTime GateInDateTime,
    string GateInDriverName,
    string GateInDriverContact,
    string? GateInOdometerReading,
    int? GateInFuelLevel,
    string? GateInProblemShared
);

/// <summary>
/// Request to complete gate out for a vehicle visit
/// </summary>
public record GateOutRequest(
    string GateOutDriverName,
    string GateOutDriverContact,
    DateTime? GateOutDateTime,
    string? GateOutOdometerReading,
    int? GateOutFuelLevel
);

/// <summary>
/// Request to update a vehicle visit
/// </summary>
public record UpdateVehicleVisitRequest(
    // Gate In fields
    string? GateInDriverName,
    string? GateInDriverContact,
    string? GateInOdometerReading,
    int? GateInFuelLevel,
    string? GateInProblemShared,
    // Gate Out fields
    string? GateOutDriverName,
    string? GateOutDriverContact,
    string? GateOutOdometerReading,
    int? GateOutFuelLevel
);

/// <summary>
/// Vehicle visit response DTO
/// </summary>
public record VehicleVisitResponse(
    int Id,
    int VehicleId,
    int WorkshopOwnerId,
    string Status,
    // Gate In details
    DateTime GateInDateTime,
    string GateInDriverName,
    string GateInDriverContact,
    string? GateInOdometerReading,
    int? GateInFuelLevel,
    string? GateInProblemShared,
    string? GateInProblemAudioUrl,
    List<string>? GateInImages,
    // Gate Out details
    DateTime? GateOutDateTime,
    string? GateOutDriverName,
    string? GateOutDriverContact,
    string? GateOutOdometerReading,
    int? GateOutFuelLevel,
    List<string>? GateOutImages,
    // Vehicle info (for convenience)
    VehicleBasicInfo? Vehicle,
    // Timestamps
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Basic vehicle info for embedding in visit response
/// </summary>
public record VehicleBasicInfo(
    int Id,
    string PlateNumber,
    string? Brand,
    string? Model,
    int? Year,
    string? Variant,
    string? Specs,
    string OwnerName,
    string ContactNumber
);

/// <summary>
/// Vehicle visit list response with count
/// </summary>
public record VehicleVisitListResponse(
    List<VehicleVisitResponse> Visits,
    int TotalCount
);

/// <summary>
/// Summary of vehicles currently in workshop
/// </summary>
public record WorkshopVehicleSummary(
    int TotalVehiclesIn,
    int TotalVehiclesOut,
    List<VehicleVisitResponse> CurrentVehicles
);
