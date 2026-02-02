using System.ComponentModel.DataAnnotations;

namespace ETNA.Api.Models.DTOs;

/// <summary>
/// DTO for creating a new sales person / team member
/// </summary>
public class CreateSalesPersonRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Sales Person";
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    [StringLength(100)]
    public string? WorkingHours { get; set; }
    
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    
    // Permissions
    public bool CanViewOrders { get; set; } = false;
    public bool CanCreateOrders { get; set; } = false;
    public bool CanEditOrders { get; set; } = false;
    public bool CanDeleteOrders { get; set; } = false;
    public bool CanViewCustomers { get; set; } = false;
    public bool CanEditCustomers { get; set; } = false;
    public bool CanDeleteCustomers { get; set; } = false;
    public bool CanViewInvoices { get; set; } = false;
    public bool CanCreateInvoices { get; set; } = false;
    public bool CanEditInvoices { get; set; } = false;
    public bool CanManageTeam { get; set; } = false;
    public bool CanViewReports { get; set; } = false;
    public bool CanApproveOrders { get; set; } = false;
    public bool CanExportData { get; set; } = false;
    public bool CanSystemSettings { get; set; } = false;
    public bool CanUpdateOrderStatus { get; set; } = false;
    public bool CanUpdateInventory { get; set; } = false;
    public bool CanViewStock { get; set; } = false;
    public bool CanProcessShipments { get; set; } = false;
    
    // Assigned entities (IDs)
    public List<int> AssignedWorkshopOwnerIds { get; set; } = new();
    public List<int> AssignedAreaIds { get; set; } = new();
    public List<int> AssignedBrandIds { get; set; } = new();
    public List<int> ReportingTeamMemberIds { get; set; } = new(); // For managers
}

/// <summary>
/// DTO for updating a sales person / team member
/// </summary>
public class UpdateSalesPersonRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Sales Person";
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    [StringLength(100)]
    public string? WorkingHours { get; set; }
    
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    
    // Permissions
    public bool CanViewOrders { get; set; } = false;
    public bool CanCreateOrders { get; set; } = false;
    public bool CanEditOrders { get; set; } = false;
    public bool CanDeleteOrders { get; set; } = false;
    public bool CanViewCustomers { get; set; } = false;
    public bool CanEditCustomers { get; set; } = false;
    public bool CanDeleteCustomers { get; set; } = false;
    public bool CanViewInvoices { get; set; } = false;
    public bool CanCreateInvoices { get; set; } = false;
    public bool CanEditInvoices { get; set; } = false;
    public bool CanManageTeam { get; set; } = false;
    public bool CanViewReports { get; set; } = false;
    public bool CanApproveOrders { get; set; } = false;
    public bool CanExportData { get; set; } = false;
    public bool CanSystemSettings { get; set; } = false;
    public bool CanUpdateOrderStatus { get; set; } = false;
    public bool CanUpdateInventory { get; set; } = false;
    public bool CanViewStock { get; set; } = false;
    public bool CanProcessShipments { get; set; } = false;
    
    // Assigned entities (IDs)
    public List<int> AssignedWorkshopOwnerIds { get; set; } = new();
    public List<int> AssignedAreaIds { get; set; } = new();
    public List<int> AssignedBrandIds { get; set; } = new();
    public List<int> ReportingTeamMemberIds { get; set; } = new();
}

/// <summary>
/// DTO for sales person response with all details
/// </summary>
public class SalesPersonResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? WorkingHours { get; set; }
    public string? PhotoUrl { get; set; }
    
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    
    // Permissions
    public bool CanViewOrders { get; set; }
    public bool CanCreateOrders { get; set; }
    public bool CanEditOrders { get; set; }
    public bool CanDeleteOrders { get; set; }
    public bool CanViewCustomers { get; set; }
    public bool CanEditCustomers { get; set; }
    public bool CanDeleteCustomers { get; set; }
    public bool CanViewInvoices { get; set; }
    public bool CanCreateInvoices { get; set; }
    public bool CanEditInvoices { get; set; }
    public bool CanManageTeam { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanApproveOrders { get; set; }
    public bool CanExportData { get; set; }
    public bool CanSystemSettings { get; set; }
    public bool CanUpdateOrderStatus { get; set; }
    public bool CanUpdateInventory { get; set; }
    public bool CanViewStock { get; set; }
    public bool CanProcessShipments { get; set; }
    
    // Connected entities
    public List<WorkshopOwnerBasicInfo> AssignedWorkshopOwners { get; set; } = new();
    public List<AreaBasicInfo> AssignedAreas { get; set; } = new();
    public List<BrandBasicInfo> AssignedBrands { get; set; } = new();
    public List<SalesPersonBasicInfo> ReportingTeamMembers { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Basic info for workshop owner in nested response
/// </summary>
public class WorkshopOwnerBasicInfo
{
    public int Id { get; set; }
    public string WorkshopName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? City { get; set; }
}

/// <summary>
/// Basic info for area in nested response
/// </summary>
public class AreaBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public List<string> Cities { get; set; } = new();
}

/// <summary>
/// Basic info for brand in nested response
/// </summary>
public class BrandBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

/// <summary>
/// Basic info for sales person in nested response
/// </summary>
public class SalesPersonBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
}

/// <summary>
/// DTO for sales person list (lighter version without nested entities)
/// </summary>
public class SalesPersonListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int AssignedCustomersCount { get; set; }
    public int AssignedAreasCount { get; set; }
    public int AssignedBrandsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
