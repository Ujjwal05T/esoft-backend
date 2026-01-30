namespace ETNA.Api.Models.DTOs;

// Request DTO for creating a job card
public record CreateJobCardRequest(
    int VehicleId,
    int? VehicleVisitId,
    string JobCategory,
    int? AssignedStaffId,
    string? Remark,
    string? Priority = "Normal"
);

// Request DTO for updating a job card
public record UpdateJobCardRequest(
    string? JobCategory,
    int? AssignedStaffId,
    string? Remark,
    string? Status,
    string? Priority,
    decimal? EstimatedCost,
    decimal? ActualCost,
    int? EstimatedDuration
);

// Response DTO for job card
public record JobCardResponse(
    int Id,
    int VehicleId,
    int? VehicleVisitId,
    int WorkshopOwnerId,
    string JobCategory,
    int? AssignedStaffId,
    string? AssignedStaffName,
    string? Remark,
    string? AudioUrl,
    List<string>? Images,
    List<string>? Videos,
    string Status,
    string Priority,
    decimal? EstimatedCost,
    decimal? ActualCost,
    int? EstimatedDuration,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    VehicleBasicInfo? Vehicle
);

// List response
public record JobCardListResponse(
    List<JobCardResponse> JobCards,
    int TotalCount
);
