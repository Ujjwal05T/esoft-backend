namespace ETNA.Api.Models.DTOs;

// ========== REQUEST DTOs ==========

public record CreateQuoteItemRequest(
    int InquiryItemId,
    string PartName,
    string PartNumber,
    string Brand,
    string Description,
    int Quantity,
    decimal Mrp,
    decimal UnitPrice,
    string Availability,
    string? EstimatedDelivery
);

public record CreateQuoteRequest(
    int InquiryId,
    int VehicleId,
    int WorkshopOwnerId,
    int? CreatedByStaffId,
    List<CreateQuoteItemRequest> Items,
    decimal PackingCharges,
    decimal ForwardingCharges,
    decimal ShippingCharges,
    DateTime? ExpiresAt
);

public record UpdateQuoteStatusRequest(
    string Status
);

// ========== RESPONSE DTOs ==========

public record QuoteItemResponse(
    int Id,
    int InquiryItemId,
    string PartName,
    string PartNumber,
    string Brand,
    string Description,
    int Quantity,
    decimal Mrp,
    decimal UnitPrice,
    string Availability,
    DateTime? EstimatedDelivery,
    DateTime CreatedAt
);

public record QuoteResponse(
    int Id,
    string QuoteNumber,
    int InquiryId,
    int VehicleId,
    int WorkshopOwnerId,
    string? InquiryNumber,
    string? VehicleName,
    string? PlateNumber,
    string? WorkshopName,
    decimal PackingCharges,
    decimal ForwardingCharges,
    decimal ShippingCharges,
    decimal TotalAmount,
    string Status,
    List<QuoteItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ExpiresAt
);

public record QuoteListResponse(
    List<QuoteResponse> Quotes,
    int TotalCount
);
