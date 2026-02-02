namespace ETNA.Api.Models.Entities;

/// <summary>
/// Represents a sales person / team member entity
/// </summary>
public class SalesPerson
{
    public int Id { get; set; }
    
    // Basic Info
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Sales Person"; // 'Sales Person', 'Manager', 'Admin', 'Backend Support', 'Warehouse Staff'
    public string? Department { get; set; }
    public string Status { get; set; } = "Active"; // 'Active', 'Inactive'
    public string? WorkingHours { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PasswordHash { get; set; }
    
    // Manager Reference (for team hierarchy)
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    
    // Assigned entities as JSON arrays
    public string? AssignedWorkshopOwnerIds { get; set; } // JSON: [1, 2, 3]
    public string? AssignedAreaIds { get; set; }          // JSON: [1, 5, 8]
    public string? AssignedBrandIds { get; set; }         // JSON: [2, 4, 7]
    
    // Permissions (19 boolean fields to avoid joins)
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
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
