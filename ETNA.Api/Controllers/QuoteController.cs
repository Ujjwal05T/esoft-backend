using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuoteController : ControllerBase
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly IInquiryRepository _inquiryRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IWorkshopOwnerRepository _workshopOwnerRepository;
    private readonly ILogger<QuoteController> _logger;

    public QuoteController(
        IQuoteRepository quoteRepository,
        IInquiryRepository inquiryRepository,
        IVehicleRepository vehicleRepository,
        IWorkshopOwnerRepository workshopOwnerRepository,
        ILogger<QuoteController> logger)
    {
        _quoteRepository = quoteRepository;
        _inquiryRepository = inquiryRepository;
        _vehicleRepository = vehicleRepository;
        _workshopOwnerRepository = workshopOwnerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Create a new quote for an inquiry
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<QuoteResponse>> CreateQuote([FromBody] CreateQuoteRequest request)
    {
        try
        {
            // Validate inquiry exists
            var inquiry = await _inquiryRepository.GetInquiryByIdAsync(request.InquiryId);
            if (inquiry == null)
            {
                return NotFound(new { message = "Inquiry not found" });
            }

            // Validate vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found" });
            }

            // Generate quote number
            var quoteNumber = await _quoteRepository.GenerateQuoteNumberAsync(request.WorkshopOwnerId);

            // Calculate total
            var partsSubtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
            var totalAmount = partsSubtotal + request.PackingCharges + request.ForwardingCharges + request.ShippingCharges;

            // Create quote
            var quote = new Quote
            {
                QuoteNumber = quoteNumber,
                InquiryId = request.InquiryId,
                VehicleId = request.VehicleId,
                WorkshopOwnerId = request.WorkshopOwnerId,
                CreatedByStaffId = request.CreatedByStaffId,
                PackingCharges = request.PackingCharges,
                ForwardingCharges = request.ForwardingCharges,
                ShippingCharges = request.ShippingCharges,
                TotalAmount = totalAmount,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt
            };

            var quoteId = await _quoteRepository.CreateQuoteAsync(quote);
            quote.Id = quoteId;

            // Create quote items
            var itemResponses = new List<QuoteItemResponse>();
            foreach (var itemRequest in request.Items)
            {
                DateTime? estimatedDelivery = null;
                if (!string.IsNullOrEmpty(itemRequest.EstimatedDelivery) && DateTime.TryParse(itemRequest.EstimatedDelivery, out var parsed))
                {
                    estimatedDelivery = parsed;
                }

                var item = new QuoteItem
                {
                    QuoteId = quoteId,
                    InquiryItemId = itemRequest.InquiryItemId,
                    PartName = itemRequest.PartName,
                    PartNumber = itemRequest.PartNumber,
                    Brand = itemRequest.Brand,
                    Description = itemRequest.Description,
                    Quantity = itemRequest.Quantity,
                    Mrp = itemRequest.Mrp,
                    UnitPrice = itemRequest.UnitPrice,
                    Availability = itemRequest.Availability,
                    EstimatedDelivery = estimatedDelivery,
                    CreatedAt = DateTime.UtcNow
                };

                var itemId = await _quoteRepository.CreateQuoteItemAsync(item);
                item.Id = itemId;

                itemResponses.Add(new QuoteItemResponse(
                    item.Id,
                    item.InquiryItemId,
                    item.PartName,
                    item.PartNumber,
                    item.Brand,
                    item.Description,
                    item.Quantity,
                    item.Mrp,
                    item.UnitPrice,
                    item.Availability,
                    item.EstimatedDelivery,
                    item.CreatedAt
                ));
            }

            var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(request.WorkshopOwnerId);

            var response = new QuoteResponse(
                quote.Id,
                quote.QuoteNumber,
                quote.InquiryId,
                quote.VehicleId,
                quote.WorkshopOwnerId,
                inquiry.InquiryNumber,
                vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
                vehicle?.PlateNumber,
                workshopOwner?.WorkshopName,
                quote.PackingCharges,
                quote.ForwardingCharges,
                quote.ShippingCharges,
                quote.TotalAmount,
                quote.Status,
                itemResponses,
                quote.CreatedAt,
                quote.UpdatedAt,
                quote.ExpiresAt
            );

            return CreatedAtAction(nameof(GetQuoteById), new { id = quoteId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quote");
            return StatusCode(500, new { message = "Error creating quote", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all quotes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<QuoteListResponse>> GetAllQuotes()
    {
        try
        {
            var quotes = await _quoteRepository.GetAllQuotesAsync();
            var responses = await BuildQuoteResponseList(quotes);
            return Ok(new QuoteListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving quotes", error = ex.Message });
        }
    }

    /// <summary>
    /// Get quote by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<QuoteResponse>> GetQuoteById(int id)
    {
        try
        {
            var quote = await _quoteRepository.GetQuoteByIdAsync(id);
            if (quote == null)
            {
                return NotFound(new { message = "Quote not found" });
            }

            var response = await BuildQuoteResponse(quote);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving quote", error = ex.Message });
        }
    }

    /// <summary>
    /// Get quotes by inquiry ID
    /// </summary>
    [HttpGet("inquiry/{inquiryId}")]
    public async Task<ActionResult<QuoteListResponse>> GetQuotesByInquiryId(int inquiryId)
    {
        try
        {
            var quotes = await _quoteRepository.GetQuotesByInquiryIdAsync(inquiryId);
            var responses = await BuildQuoteResponseList(quotes);
            return Ok(new QuoteListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving quotes", error = ex.Message });
        }
    }

    /// <summary>
    /// Get quotes by vehicle ID
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    public async Task<ActionResult<QuoteListResponse>> GetQuotesByVehicleId(int vehicleId)
    {
        try
        {
            var quotes = await _quoteRepository.GetQuotesByVehicleIdAsync(vehicleId);
            var responses = await BuildQuoteResponseList(quotes);
            return Ok(new QuoteListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving quotes", error = ex.Message });
        }
    }

    /// <summary>
    /// Get quotes by workshop owner ID
    /// </summary>
    [HttpGet("workshop/{workshopOwnerId}")]
    public async Task<ActionResult<QuoteListResponse>> GetQuotesByWorkshopOwnerId(int workshopOwnerId)
    {
        try
        {
            var quotes = await _quoteRepository.GetQuotesByWorkshopOwnerIdAsync(workshopOwnerId);
            var responses = await BuildQuoteResponseList(quotes);
            return Ok(new QuoteListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving quotes", error = ex.Message });
        }
    }

    /// <summary>
    /// Update quote status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateQuoteStatus(int id, [FromBody] UpdateQuoteStatusRequest request)
    {
        try
        {
            var quote = await _quoteRepository.GetQuoteByIdAsync(id);
            if (quote == null)
            {
                return NotFound(new { message = "Quote not found" });
            }

            var validStatuses = new[] { "pending", "approved", "rejected" };
            if (!validStatuses.Contains(request.Status.ToLower()))
            {
                return BadRequest(new { message = "Invalid status. Must be: pending, approved, rejected" });
            }

            var success = await _quoteRepository.UpdateQuoteStatusAsync(id, request.Status.ToLower());
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to update quote status" });
            }

            return Ok(new { message = "Quote status updated successfully", status = request.Status.ToLower() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating quote status", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete quote
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuote(int id)
    {
        try
        {
            var quote = await _quoteRepository.GetQuoteByIdAsync(id);
            if (quote == null)
            {
                return NotFound(new { message = "Quote not found" });
            }

            var success = await _quoteRepository.DeleteQuoteAsync(id);
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to delete quote" });
            }

            return Ok(new { message = "Quote deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting quote", error = ex.Message });
        }
    }

    // ========== PRIVATE HELPERS ==========

    private async Task<QuoteResponse> BuildQuoteResponse(Quote quote)
    {
        var items = await _quoteRepository.GetQuoteItemsByQuoteIdAsync(quote.Id);
        var inquiry = await _inquiryRepository.GetInquiryByIdAsync(quote.InquiryId);
        var vehicle = await _vehicleRepository.GetByIdAsync(quote.VehicleId);
        var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(quote.WorkshopOwnerId);

        var itemResponses = items.Select(item => new QuoteItemResponse(
            item.Id,
            item.InquiryItemId,
            item.PartName,
            item.PartNumber,
            item.Brand,
            item.Description,
            item.Quantity,
            item.Mrp,
            item.UnitPrice,
            item.Availability,
            item.EstimatedDelivery,
            item.CreatedAt
        )).ToList();

        return new QuoteResponse(
            quote.Id,
            quote.QuoteNumber,
            quote.InquiryId,
            quote.VehicleId,
            quote.WorkshopOwnerId,
            inquiry?.InquiryNumber,
            vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
            vehicle?.PlateNumber,
            workshopOwner?.WorkshopName,
            quote.PackingCharges,
            quote.ForwardingCharges,
            quote.ShippingCharges,
            quote.TotalAmount,
            quote.Status,
            itemResponses,
            quote.CreatedAt,
            quote.UpdatedAt,
            quote.ExpiresAt
        );
    }

    private async Task<List<QuoteResponse>> BuildQuoteResponseList(List<Quote> quotes)
    {
        var responses = new List<QuoteResponse>();
        foreach (var quote in quotes)
        {
            responses.Add(await BuildQuoteResponse(quote));
        }
        return responses;
    }
}
