namespace ETNA.Api.Models.Entities;

public enum JobCardStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public enum JobPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public class JobCard
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int? VehicleVisitId { get; set; }
    public int WorkshopOwnerId { get; set; }
    public string JobCategory { get; set; } = string.Empty;
    public int? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
    public string? Remark { get; set; }
    public string? AudioUrl { get; set; }
    public string? Images { get; set; } // JSON array
    public string? Videos { get; set; } // JSON array
    public JobCardStatus Status { get; set; } = JobCardStatus.Pending;
    public JobPriority Priority { get; set; } = JobPriority.Normal;
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public int? EstimatedDuration { get; set; } // in minutes
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
