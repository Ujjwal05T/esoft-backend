using ETNA.Api.Models.Entities;

namespace ETNA.Api.Models.DTOs;

// ========== REQUEST DTOs ==========

public record CreateInquiryItemRequest(
    string PartName,
    string PreferredBrand,
    int Quantity,
    string Remark,
    string? AudioUrl,
    int? AudioDuration,
    string? Image1Url,
    string? Image2Url,
    string? Image3Url
);

public record CreateInquiryRequest(
    int VehicleId,
    int? VehicleVisitId,
    int WorkshopOwnerId,
    int? RequestedByStaffId,
    string JobCategory,
    List<CreateInquiryItemRequest> Items
);

public record UpdateInquiryStatusRequest(
    string Status
);

// ========== RESPONSE DTOs ==========

public record InquiryItemResponse(
    int Id,
    string PartName,
    string PreferredBrand,
    int Quantity,
    string Remark,
    string? AudioUrl,
    int? AudioDuration,
    string? Image1Url,
    string? Image2Url,
    string? Image3Url,
    DateTime CreatedAt
);

public record InquiryResponse(
    int Id,
    int VehicleId,
    int? VehicleVisitId,
    int WorkshopOwnerId,
    int? RequestedByStaffId,
    string InquiryNumber,
    string JobCategory,
    string Status,
    DateTime PlacedDate,
    DateTime? ClosedDate,
    DateTime? DeclinedDate,
    List<InquiryItemResponse> Items,
    // Additional info from joins
    string? VehicleName,
    string? NumberPlate,
    string? RequestedByName,
    string? WorkshopName
);

public record InquiryListResponse(
    List<InquiryResponse> Inquiries,
    int TotalCount
);

// ========== HELPER METHODS ==========

public static class InquiryExtensions
{
    public static string ToStatusString(this InquiryStatus status)
    {
        return status switch
        {
            InquiryStatus.Open => "open",
            InquiryStatus.Closed => "closed",
            InquiryStatus.Approved => "approved",
            InquiryStatus.Requested => "requested",
            InquiryStatus.Declined => "declined",
            _ => "open"
        };
    }

    public static InquiryStatus ToInquiryStatus(this string status)
    {
        return status.ToLower() switch
        {
            "open" => InquiryStatus.Open,
            "closed" => InquiryStatus.Closed,
            "approved" => InquiryStatus.Approved,
            "requested" => InquiryStatus.Requested,
            "declined" => InquiryStatus.Declined,
            _ => InquiryStatus.Open
        };
    }
}
