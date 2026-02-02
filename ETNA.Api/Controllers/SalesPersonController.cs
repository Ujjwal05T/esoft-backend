using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Controllers;

/// <summary>
/// API Controller for Sales Person / Team Member management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesPersonController : ControllerBase
{
    private readonly ISalesPersonRepository _salesPersonRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IWorkshopOwnerRepository _workshopOwnerRepository;
    private readonly ILogger<SalesPersonController> _logger;

    public SalesPersonController(
        ISalesPersonRepository salesPersonRepository,
        IAreaRepository areaRepository,
        IBrandRepository brandRepository,
        IWorkshopOwnerRepository workshopOwnerRepository,
        ILogger<SalesPersonController> logger)
    {
        _salesPersonRepository = salesPersonRepository;
        _areaRepository = areaRepository;
        _brandRepository = brandRepository;
        _workshopOwnerRepository = workshopOwnerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all sales persons / team members
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesPersonListItem>>> GetAll()
    {
        try
        {
            var salesPersons = await _salesPersonRepository.GetAllAsync();
            var response = salesPersons.Select(sp => new SalesPersonListItem
            {
                Id = sp.Id,
                Name = sp.Name,
                Email = sp.Email,
                Phone = sp.Phone,
                Role = sp.Role,
                Department = sp.Department,
                Status = sp.Status,
                PhotoUrl = sp.PhotoUrl,
                ManagerId = sp.ManagerId,
                ManagerName = sp.ManagerName,
                AssignedCustomersCount = ParseJsonArray(sp.AssignedWorkshopOwnerIds).Count,
                AssignedAreasCount = ParseJsonArray(sp.AssignedAreaIds).Count,
                AssignedBrandsCount = ParseJsonArray(sp.AssignedBrandIds).Count,
                CreatedAt = sp.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all sales persons");
            return StatusCode(500, new { error = "An error occurred while fetching sales persons" });
        }
    }

    /// <summary>
    /// Get sales persons by role
    /// </summary>
    [HttpGet("by-role/{role}")]
    public async Task<ActionResult<IEnumerable<SalesPersonListItem>>> GetByRole(string role)
    {
        try
        {
            var salesPersons = await _salesPersonRepository.GetByRoleAsync(role);
            var response = salesPersons.Select(sp => new SalesPersonListItem
            {
                Id = sp.Id,
                Name = sp.Name,
                Email = sp.Email,
                Phone = sp.Phone,
                Role = sp.Role,
                Department = sp.Department,
                Status = sp.Status,
                PhotoUrl = sp.PhotoUrl,
                ManagerId = sp.ManagerId,
                ManagerName = sp.ManagerName,
                CreatedAt = sp.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales persons by role {Role}", role);
            return StatusCode(500, new { error = "An error occurred while fetching sales persons" });
        }
    }

    /// <summary>
    /// Get sales person by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SalesPersonResponse>> GetById(int id)
    {
        try
        {
            var salesPerson = await _salesPersonRepository.GetByIdAsync(id);
            if (salesPerson == null)
            {
                return NotFound(new { error = "Sales person not found" });
            }

            var response = await MapToFullResponse(salesPerson);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales person {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the sales person" });
        }
    }

    /// <summary>
    /// Create a new sales person / team member
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SalesPersonResponse>> Create([FromBody] CreateSalesPersonRequest request)
    {
        try
        {
            // Check if email already exists
            var existingEmail = await _salesPersonRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { error = "A sales person with this email already exists" });
            }

            // Check if phone already exists
            var existingPhone = await _salesPersonRepository.GetByPhoneAsync(request.Phone);
            if (existingPhone != null)
            {
                return BadRequest(new { error = "A sales person with this phone number already exists" });
            }

            var salesPerson = new SalesPerson
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Role = request.Role,
                Department = request.Department,
                Status = request.Status,
                WorkingHours = request.WorkingHours,
                ManagerId = request.ManagerId,
                ManagerName = request.ManagerName,
                AssignedWorkshopOwnerIds = SerializeToJson(request.AssignedWorkshopOwnerIds),
                AssignedAreaIds = SerializeToJson(request.AssignedAreaIds),
                AssignedBrandIds = SerializeToJson(request.AssignedBrandIds),
                CanViewOrders = request.CanViewOrders,
                CanCreateOrders = request.CanCreateOrders,
                CanEditOrders = request.CanEditOrders,
                CanDeleteOrders = request.CanDeleteOrders,
                CanViewCustomers = request.CanViewCustomers,
                CanEditCustomers = request.CanEditCustomers,
                CanDeleteCustomers = request.CanDeleteCustomers,
                CanViewInvoices = request.CanViewInvoices,
                CanCreateInvoices = request.CanCreateInvoices,
                CanEditInvoices = request.CanEditInvoices,
                CanManageTeam = request.CanManageTeam,
                CanViewReports = request.CanViewReports,
                CanApproveOrders = request.CanApproveOrders,
                CanExportData = request.CanExportData,
                CanSystemSettings = request.CanSystemSettings,
                CanUpdateOrderStatus = request.CanUpdateOrderStatus,
                CanUpdateInventory = request.CanUpdateInventory,
                CanViewStock = request.CanViewStock,
                CanProcessShipments = request.CanProcessShipments,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _salesPersonRepository.CreateAsync(salesPerson);

            // Update reporting team members' manager reference
            if (request.ReportingTeamMemberIds.Any())
            {
                foreach (var teamMemberId in request.ReportingTeamMemberIds)
                {
                    var teamMember = await _salesPersonRepository.GetByIdAsync(teamMemberId);
                    if (teamMember != null)
                    {
                        teamMember.ManagerId = created.Id;
                        teamMember.ManagerName = created.Name;
                        await _salesPersonRepository.UpdateAsync(teamMember);
                    }
                }
            }

            var response = await MapToFullResponse(created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales person");
            return StatusCode(500, new { error = "An error occurred while creating the sales person" });
        }
    }

    /// <summary>
    /// Update an existing sales person / team member
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SalesPersonResponse>> Update(int id, [FromBody] UpdateSalesPersonRequest request)
    {
        try
        {
            var existing = await _salesPersonRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Sales person not found" });
            }

            // Check email conflict
            var emailConflict = await _salesPersonRepository.GetByEmailAsync(request.Email);
            if (emailConflict != null && emailConflict.Id != id)
            {
                return BadRequest(new { error = "A sales person with this email already exists" });
            }

            // Check phone conflict
            var phoneConflict = await _salesPersonRepository.GetByPhoneAsync(request.Phone);
            if (phoneConflict != null && phoneConflict.Id != id)
            {
                return BadRequest(new { error = "A sales person with this phone number already exists" });
            }

            existing.Name = request.Name;
            existing.Email = request.Email;
            existing.Phone = request.Phone;
            existing.Role = request.Role;
            existing.Department = request.Department;
            existing.Status = request.Status;
            existing.WorkingHours = request.WorkingHours;
            existing.ManagerId = request.ManagerId;
            existing.ManagerName = request.ManagerName;
            existing.AssignedWorkshopOwnerIds = SerializeToJson(request.AssignedWorkshopOwnerIds);
            existing.AssignedAreaIds = SerializeToJson(request.AssignedAreaIds);
            existing.AssignedBrandIds = SerializeToJson(request.AssignedBrandIds);
            existing.CanViewOrders = request.CanViewOrders;
            existing.CanCreateOrders = request.CanCreateOrders;
            existing.CanEditOrders = request.CanEditOrders;
            existing.CanDeleteOrders = request.CanDeleteOrders;
            existing.CanViewCustomers = request.CanViewCustomers;
            existing.CanEditCustomers = request.CanEditCustomers;
            existing.CanDeleteCustomers = request.CanDeleteCustomers;
            existing.CanViewInvoices = request.CanViewInvoices;
            existing.CanCreateInvoices = request.CanCreateInvoices;
            existing.CanEditInvoices = request.CanEditInvoices;
            existing.CanManageTeam = request.CanManageTeam;
            existing.CanViewReports = request.CanViewReports;
            existing.CanApproveOrders = request.CanApproveOrders;
            existing.CanExportData = request.CanExportData;
            existing.CanSystemSettings = request.CanSystemSettings;
            existing.CanUpdateOrderStatus = request.CanUpdateOrderStatus;
            existing.CanUpdateInventory = request.CanUpdateInventory;
            existing.CanViewStock = request.CanViewStock;
            existing.CanProcessShipments = request.CanProcessShipments;

            var updated = await _salesPersonRepository.UpdateAsync(existing);

            // Update reporting team if this is a manager
            if (request.Role == "Manager" && request.ReportingTeamMemberIds.Any())
            {
                // Clear previous team members' manager reference
                var previousTeam = await _salesPersonRepository.GetByManagerIdAsync(id);
                foreach (var member in previousTeam)
                {
                    if (!request.ReportingTeamMemberIds.Contains(member.Id))
                    {
                        member.ManagerId = null;
                        member.ManagerName = null;
                        await _salesPersonRepository.UpdateAsync(member);
                    }
                }

                // Set new team members' manager reference
                foreach (var teamMemberId in request.ReportingTeamMemberIds)
                {
                    var teamMember = await _salesPersonRepository.GetByIdAsync(teamMemberId);
                    if (teamMember != null && teamMember.ManagerId != id)
                    {
                        teamMember.ManagerId = id;
                        teamMember.ManagerName = updated!.Name;
                        await _salesPersonRepository.UpdateAsync(teamMember);
                    }
                }
            }

            var response = await MapToFullResponse(updated!);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales person {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the sales person" });
        }
    }

    /// <summary>
    /// Set sales person status (Active/Inactive)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> SetStatus(int id, [FromBody] string status)
    {
        try
        {
            var existing = await _salesPersonRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Sales person not found" });
            }

            if (status != "Active" && status != "Inactive")
            {
                return BadRequest(new { error = "Status must be 'Active' or 'Inactive'" });
            }

            await _salesPersonRepository.SetStatusAsync(id, status);
            return Ok(new { message = $"Status updated to {status}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for sales person {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating status" });
        }
    }

    /// <summary>
    /// Delete a sales person
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _salesPersonRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Sales person not found" });
            }

            // Clear manager references from any team members
            var teamMembers = await _salesPersonRepository.GetByManagerIdAsync(id);
            foreach (var member in teamMembers)
            {
                member.ManagerId = null;
                member.ManagerName = null;
                await _salesPersonRepository.UpdateAsync(member);
            }

            await _salesPersonRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales person {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the sales person" });
        }
    }

    /// <summary>
    /// Get team members reporting to a manager
    /// </summary>
    [HttpGet("{id}/team")]
    public async Task<ActionResult<IEnumerable<SalesPersonBasicInfo>>> GetTeamMembers(int id)
    {
        try
        {
            var existing = await _salesPersonRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Sales person not found" });
            }

            var teamMembers = await _salesPersonRepository.GetByManagerIdAsync(id);
            var response = teamMembers.Select(m => new SalesPersonBasicInfo
            {
                Id = m.Id,
                Name = m.Name,
                Role = m.Role,
                Department = m.Department
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching team members" });
        }
    }

    // Helper methods for JSON serialization
    private static List<int> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<int>();
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    private static string? SerializeToJson(List<int>? ids)
    {
        if (ids == null || !ids.Any()) return null;
        return JsonSerializer.Serialize(ids);
    }

    private async Task<SalesPersonResponse> MapToFullResponse(SalesPerson salesPerson)
    {
        var response = new SalesPersonResponse
        {
            Id = salesPerson.Id,
            Name = salesPerson.Name,
            Email = salesPerson.Email,
            Phone = salesPerson.Phone,
            Role = salesPerson.Role,
            Department = salesPerson.Department,
            Status = salesPerson.Status,
            WorkingHours = salesPerson.WorkingHours,
            PhotoUrl = salesPerson.PhotoUrl,
            ManagerId = salesPerson.ManagerId,
            ManagerName = salesPerson.ManagerName,
            CanViewOrders = salesPerson.CanViewOrders,
            CanCreateOrders = salesPerson.CanCreateOrders,
            CanEditOrders = salesPerson.CanEditOrders,
            CanDeleteOrders = salesPerson.CanDeleteOrders,
            CanViewCustomers = salesPerson.CanViewCustomers,
            CanEditCustomers = salesPerson.CanEditCustomers,
            CanDeleteCustomers = salesPerson.CanDeleteCustomers,
            CanViewInvoices = salesPerson.CanViewInvoices,
            CanCreateInvoices = salesPerson.CanCreateInvoices,
            CanEditInvoices = salesPerson.CanEditInvoices,
            CanManageTeam = salesPerson.CanManageTeam,
            CanViewReports = salesPerson.CanViewReports,
            CanApproveOrders = salesPerson.CanApproveOrders,
            CanExportData = salesPerson.CanExportData,
            CanSystemSettings = salesPerson.CanSystemSettings,
            CanUpdateOrderStatus = salesPerson.CanUpdateOrderStatus,
            CanUpdateInventory = salesPerson.CanUpdateInventory,
            CanViewStock = salesPerson.CanViewStock,
            CanProcessShipments = salesPerson.CanProcessShipments,
            CreatedAt = salesPerson.CreatedAt,
            UpdatedAt = salesPerson.UpdatedAt
        };

        // Get assigned workshop owners
        var workshopOwnerIds = ParseJsonArray(salesPerson.AssignedWorkshopOwnerIds);
        foreach (var woId in workshopOwnerIds)
        {
            var wo = await _workshopOwnerRepository.GetByIdAsync(woId);
            if (wo != null)
            {
                response.AssignedWorkshopOwners.Add(new WorkshopOwnerBasicInfo
                {
                    Id = wo.Id,
                    WorkshopName = wo.WorkshopName,
                    ContactPerson = wo.OwnerName,
                    City = wo.City
                });
            }
        }

        // Get assigned areas
        var areaIds = ParseJsonArray(salesPerson.AssignedAreaIds);
        foreach (var areaId in areaIds)
        {
            var area = await _areaRepository.GetByIdAsync(areaId);
            if (area != null)
            {
                List<string> cities;
                try
                {
                    cities = JsonSerializer.Deserialize<List<string>>(area.Cities) ?? new List<string>();
                }
                catch
                {
                    cities = new List<string>();
                }

                response.AssignedAreas.Add(new AreaBasicInfo
                {
                    Id = area.Id,
                    Name = area.Name,
                    State = area.State,
                    Cities = cities
                });
            }
        }

        // Get assigned brands
        var brandIds = ParseJsonArray(salesPerson.AssignedBrandIds);
        foreach (var brandId in brandIds)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand != null)
            {
                response.AssignedBrands.Add(new BrandBasicInfo
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    LogoUrl = brand.LogoUrl
                });
            }
        }

        // Get reporting team members
        var teamMembers = await _salesPersonRepository.GetByManagerIdAsync(salesPerson.Id);
        foreach (var member in teamMembers)
        {
            response.ReportingTeamMembers.Add(new SalesPersonBasicInfo
            {
                Id = member.Id,
                Name = member.Name,
                Role = member.Role,
                Department = member.Department
            });
        }

        return response;
    }
}
