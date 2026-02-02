using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiryController : ControllerBase
{
    private readonly IInquiryRepository _inquiryRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IWorkshopStaffRepository _staffRepository;
    private readonly IWorkshopOwnerRepository _workshopOwnerRepository;

    public InquiryController(
        IInquiryRepository inquiryRepository,
        IVehicleRepository vehicleRepository,
        IWorkshopStaffRepository staffRepository,
        IWorkshopOwnerRepository workshopOwnerRepository)
    {
        _inquiryRepository = inquiryRepository;
        _vehicleRepository = vehicleRepository;
        _staffRepository = staffRepository;
        _workshopOwnerRepository = workshopOwnerRepository;
    }

    /// <summary>
    /// Create a new inquiry with items
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InquiryResponse>> CreateInquiry([FromBody] CreateInquiryRequest request)
    {
        try
        {
            // Validate vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found" });
            }

            // Generate inquiry number
            var inquiryNumber = await _inquiryRepository.GenerateInquiryNumberAsync(request.WorkshopOwnerId);

            // Create inquiry
            var inquiry = new Inquiry
            {
                VehicleId = request.VehicleId,
                VehicleVisitId = request.VehicleVisitId,
                WorkshopOwnerId = request.WorkshopOwnerId,
                RequestedByStaffId = request.RequestedByStaffId,
                InquiryNumber = inquiryNumber,
                JobCategory = request.JobCategory,
                Status = InquiryStatus.Open,
                PlacedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var inquiryId = await _inquiryRepository.CreateInquiryAsync(inquiry);
            inquiry.Id = inquiryId;

            // Create inquiry items
            var itemResponses = new List<InquiryItemResponse>();
            foreach (var itemRequest in request.Items)
            {
                var item = new InquiryItem
                {
                    InquiryId = inquiryId,
                    PartName = itemRequest.PartName,
                    PreferredBrand = itemRequest.PreferredBrand,
                    Quantity = itemRequest.Quantity,
                    Remark = itemRequest.Remark,
                    AudioUrl = itemRequest.AudioUrl,
                    AudioDuration = itemRequest.AudioDuration,
                    Image1Url = itemRequest.Image1Url,
                    Image2Url = itemRequest.Image2Url,
                    Image3Url = itemRequest.Image3Url,
                    CreatedAt = DateTime.UtcNow
                };

                var itemId = await _inquiryRepository.CreateInquiryItemAsync(item);
                item.Id = itemId;

                itemResponses.Add(new InquiryItemResponse(
                    item.Id,
                    item.PartName,
                    item.PreferredBrand,
                    item.Quantity,
                    item.Remark,
                    item.AudioUrl,
                    item.AudioDuration,
                    item.Image1Url,
                    item.Image2Url,
                    item.Image3Url,
                    item.CreatedAt
                ));
            }

            // Get staff name (if requested by staff)
            var staff = request.RequestedByStaffId.HasValue 
                ? await _staffRepository.GetByIdAsync(request.RequestedByStaffId.Value)
                : null;

            var response = new InquiryResponse(
                inquiry.Id,
                inquiry.VehicleId,
                inquiry.VehicleVisitId,
                inquiry.WorkshopOwnerId,
                inquiry.RequestedByStaffId,
                inquiry.InquiryNumber,
                inquiry.JobCategory,
                inquiry.Status.ToStatusString(),
                inquiry.PlacedDate,
                inquiry.ClosedDate,
                inquiry.DeclinedDate,
                itemResponses,
                $"{vehicle.Brand} {vehicle.Model}",
                vehicle.PlateNumber,
                staff?.Name,
                null // workshopOwnerName - not needed for POST
            );

            return CreatedAtAction(nameof(GetInquiryById), new { id = inquiryId }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating inquiry", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all inquiries (for admin portal)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<InquiryListResponse>> GetAllInquiries()
    {
        try
        {
            var inquiries = await _inquiryRepository.GetAllInquiriesAsync();

            var responses = new List<InquiryResponse>();
            foreach (var inquiry in inquiries)
            {
                var items = await _inquiryRepository.GetInquiryItemsByInquiryIdAsync(inquiry.Id);
                var vehicle = await _vehicleRepository.GetByIdAsync(inquiry.VehicleId);
                var staff = inquiry.RequestedByStaffId.HasValue
                    ? await _staffRepository.GetByIdAsync(inquiry.RequestedByStaffId.Value)
                    : null;
                var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(inquiry.WorkshopOwnerId);

                var itemResponses = items.Select(item => new InquiryItemResponse(
                    item.Id,
                    item.PartName,
                    item.PreferredBrand,
                    item.Quantity,
                    item.Remark,
                    item.AudioUrl,
                    item.AudioDuration,
                    item.Image1Url,
                    item.Image2Url,
                    item.Image3Url,
                    item.CreatedAt
                )).ToList();

                responses.Add(new InquiryResponse(
                    inquiry.Id,
                    inquiry.VehicleId,
                    inquiry.VehicleVisitId,
                    inquiry.WorkshopOwnerId,
                    inquiry.RequestedByStaffId,
                    inquiry.InquiryNumber,
                    inquiry.JobCategory,
                    inquiry.Status.ToStatusString(),
                    inquiry.PlacedDate,
                    inquiry.ClosedDate,
                    inquiry.DeclinedDate,
                    itemResponses,
                    vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
                    vehicle?.PlateNumber,
                    staff?.Name,
                    workshopOwner?.WorkshopName
                ));
            }

            return Ok(new InquiryListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving inquiries", error = ex.Message });
        }
    }

    /// <summary>
    /// Get inquiry by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InquiryResponse>> GetInquiryById(int id)
    {
        try
        {
            var inquiry = await _inquiryRepository.GetInquiryByIdAsync(id);
            if (inquiry == null)
            {
                return NotFound(new { message = "Inquiry not found" });
            }

            var items = await _inquiryRepository.GetInquiryItemsByInquiryIdAsync(id);
            var vehicle = await _vehicleRepository.GetByIdAsync(inquiry.VehicleId);
            var staff = inquiry.RequestedByStaffId.HasValue
                ? await _staffRepository.GetByIdAsync(inquiry.RequestedByStaffId.Value)
                : null;
            var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(inquiry.WorkshopOwnerId);

            var itemResponses = items.Select(item => new InquiryItemResponse(
                item.Id,
                item.PartName,
                item.PreferredBrand,
                item.Quantity,
                item.Remark,
                item.AudioUrl,
                item.AudioDuration,
                item.Image1Url,
                item.Image2Url,
                item.Image3Url,
                item.CreatedAt
            )).ToList();

            var response = new InquiryResponse(
                inquiry.Id,
                inquiry.VehicleId,
                inquiry.VehicleVisitId,
                inquiry.WorkshopOwnerId,
                inquiry.RequestedByStaffId,
                inquiry.InquiryNumber,
                inquiry.JobCategory,
                inquiry.Status.ToStatusString(),
                inquiry.PlacedDate,
                inquiry.ClosedDate,
                inquiry.DeclinedDate,
                itemResponses,
                vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
                vehicle?.PlateNumber,
                staff?.Name,
                workshopOwner?.WorkshopName
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving inquiry", error = ex.Message });
        }
    }

    /// <summary>
    /// Get inquiries by vehicle ID
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    public async Task<ActionResult<InquiryListResponse>> GetInquiriesByVehicleId(int vehicleId)
    {
        try
        {
            var inquiries = await _inquiryRepository.GetInquiriesByVehicleIdAsync(vehicleId);
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            var responses = new List<InquiryResponse>();
            foreach (var inquiry in inquiries)
            {
                var items = await _inquiryRepository.GetInquiryItemsByInquiryIdAsync(inquiry.Id);
                var staff = inquiry.RequestedByStaffId.HasValue
                    ? await _staffRepository.GetByIdAsync(inquiry.RequestedByStaffId.Value)
                    : null;
                var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(inquiry.WorkshopOwnerId);

                var itemResponses = items.Select(item => new InquiryItemResponse(
                    item.Id,
                    item.PartName,
                    item.PreferredBrand,
                    item.Quantity,
                    item.Remark,
                    item.AudioUrl,
                    item.AudioDuration,
                    item.Image1Url,
                    item.Image2Url,
                    item.Image3Url,
                    item.CreatedAt
                )).ToList();

                responses.Add(new InquiryResponse(
                    inquiry.Id,
                    inquiry.VehicleId,
                    inquiry.VehicleVisitId,
                    inquiry.WorkshopOwnerId,
                    inquiry.RequestedByStaffId,
                    inquiry.InquiryNumber,
                    inquiry.JobCategory,
                    inquiry.Status.ToStatusString(),
                    inquiry.PlacedDate,
                    inquiry.ClosedDate,
                    inquiry.DeclinedDate,
                    itemResponses,
                    vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
                    vehicle?.PlateNumber,
                    staff?.Name,
                    workshopOwner?.WorkshopName
                ));
            }

            return Ok(new InquiryListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving inquiries", error = ex.Message });
        }
    }

    /// <summary>
    /// Get inquiries by workshop owner ID
    /// </summary>
    [HttpGet("workshop/{workshopOwnerId}")]
    public async Task<ActionResult<InquiryListResponse>> GetInquiriesByWorkshopOwnerId(int workshopOwnerId)
    {
        try
        {
            var inquiries = await _inquiryRepository.GetInquiriesByWorkshopOwnerIdAsync(workshopOwnerId);

            var responses = new List<InquiryResponse>();
            foreach (var inquiry in inquiries)
            {
                var items = await _inquiryRepository.GetInquiryItemsByInquiryIdAsync(inquiry.Id);
                var vehicle = await _vehicleRepository.GetByIdAsync(inquiry.VehicleId);
                var staff = inquiry.RequestedByStaffId.HasValue
                    ? await _staffRepository.GetByIdAsync(inquiry.RequestedByStaffId.Value)
                    : null;
                var workshopOwner = await _workshopOwnerRepository.GetByIdAsync(inquiry.WorkshopOwnerId);

                var itemResponses = items.Select(item => new InquiryItemResponse(
                    item.Id,
                    item.PartName,
                    item.PreferredBrand,
                    item.Quantity,
                    item.Remark,
                    item.AudioUrl,
                    item.AudioDuration,
                    item.Image1Url,
                    item.Image2Url,
                    item.Image3Url,
                    item.CreatedAt
                )).ToList();

                responses.Add(new InquiryResponse(
                    inquiry.Id,
                    inquiry.VehicleId,
                    inquiry.VehicleVisitId,
                    inquiry.WorkshopOwnerId,
                    inquiry.RequestedByStaffId,
                    inquiry.InquiryNumber,
                    inquiry.JobCategory,
                    inquiry.Status.ToStatusString(),
                    inquiry.PlacedDate,
                    inquiry.ClosedDate,
                    inquiry.DeclinedDate,
                    itemResponses,
                    vehicle != null ? $"{vehicle.Brand} {vehicle.Model}" : null,
                    vehicle?.PlateNumber,
                    staff?.Name,
                    workshopOwner?.WorkshopName
                ));
            }

            return Ok(new InquiryListResponse(responses, responses.Count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving inquiries", error = ex.Message });
        }
    }

    /// <summary>
    /// Update inquiry status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateInquiryStatus(int id, [FromBody] UpdateInquiryStatusRequest request)
    {
        try
        {
            var inquiry = await _inquiryRepository.GetInquiryByIdAsync(id);
            if (inquiry == null)
            {
                return NotFound(new { message = "Inquiry not found" });
            }

            var status = request.Status.ToInquiryStatus();
            var success = await _inquiryRepository.UpdateInquiryStatusAsync(id, status);

            if (!success)
            {
                return StatusCode(500, new { message = "Failed to update inquiry status" });
            }

            return Ok(new { message = "Inquiry status updated successfully", status = request.Status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating inquiry status", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete inquiry
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteInquiry(int id)
    {
        try
        {
            var inquiry = await _inquiryRepository.GetInquiryByIdAsync(id);
            if (inquiry == null)
            {
                return NotFound(new { message = "Inquiry not found" });
            }

            var success = await _inquiryRepository.DeleteInquiryAsync(id);
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to delete inquiry" });
            }

            return Ok(new { message = "Inquiry deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting inquiry", error = ex.Message });
        }
    }
}
