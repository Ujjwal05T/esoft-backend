using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for SalesPerson data access using Dapper
/// </summary>
public class SalesPersonRepository : ISalesPersonRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SalesPersonRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SalesPerson> CreateAsync(SalesPerson salesPerson)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO SalesPersons (
                Name, Email, Phone, Role, Department, Status, WorkingHours, PhotoUrl, PasswordHash,
                ManagerId, ManagerName,
                AssignedWorkshopOwnerIds, AssignedAreaIds, AssignedBrandIds,
                CanViewOrders, CanCreateOrders, CanEditOrders, CanDeleteOrders,
                CanViewCustomers, CanEditCustomers, CanDeleteCustomers,
                CanViewInvoices, CanCreateInvoices, CanEditInvoices,
                CanManageTeam, CanViewReports, CanApproveOrders, CanExportData, CanSystemSettings,
                CanUpdateOrderStatus, CanUpdateInventory, CanViewStock, CanProcessShipments,
                CreatedAt
            )
            OUTPUT INSERTED.*
            VALUES (
                @Name, @Email, @Phone, @Role, @Department, @Status, @WorkingHours, @PhotoUrl, @PasswordHash,
                @ManagerId, @ManagerName,
                @AssignedWorkshopOwnerIds, @AssignedAreaIds, @AssignedBrandIds,
                @CanViewOrders, @CanCreateOrders, @CanEditOrders, @CanDeleteOrders,
                @CanViewCustomers, @CanEditCustomers, @CanDeleteCustomers,
                @CanViewInvoices, @CanCreateInvoices, @CanEditInvoices,
                @CanManageTeam, @CanViewReports, @CanApproveOrders, @CanExportData, @CanSystemSettings,
                @CanUpdateOrderStatus, @CanUpdateInventory, @CanViewStock, @CanProcessShipments,
                @CreatedAt
            )";

        var result = await connection.QuerySingleAsync<SalesPerson>(sql, salesPerson);
        return result;
    }

    public async Task<SalesPerson?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SalesPersons WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<SalesPerson>(sql, new { Id = id });
    }

    public async Task<SalesPerson?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SalesPersons WHERE Email = @Email";
        return await connection.QuerySingleOrDefaultAsync<SalesPerson>(sql, new { Email = email });
    }

    public async Task<SalesPerson?> GetByPhoneAsync(string phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SalesPersons WHERE Phone = @Phone";
        return await connection.QuerySingleOrDefaultAsync<SalesPerson>(sql, new { Phone = phone });
    }

    public async Task<IEnumerable<SalesPerson>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SalesPersons ORDER BY Name";
        return await connection.QueryAsync<SalesPerson>(sql);
    }

    public async Task<IEnumerable<SalesPerson>> GetByRoleAsync(string role)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SalesPersons 
            WHERE Role = @Role 
            ORDER BY Name";
        return await connection.QueryAsync<SalesPerson>(sql, new { Role = role });
    }

    public async Task<IEnumerable<SalesPerson>> GetByStatusAsync(string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SalesPersons 
            WHERE Status = @Status 
            ORDER BY Name";
        return await connection.QueryAsync<SalesPerson>(sql, new { Status = status });
    }

    public async Task<IEnumerable<SalesPerson>> GetByManagerIdAsync(int managerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SalesPersons 
            WHERE ManagerId = @ManagerId 
            ORDER BY Name";
        return await connection.QueryAsync<SalesPerson>(sql, new { ManagerId = managerId });
    }

    public async Task<SalesPerson?> UpdateAsync(SalesPerson salesPerson)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE SalesPersons SET
                Name = @Name,
                Email = @Email,
                Phone = @Phone,
                Role = @Role,
                Department = @Department,
                Status = @Status,
                WorkingHours = @WorkingHours,
                PhotoUrl = @PhotoUrl,
                ManagerId = @ManagerId,
                ManagerName = @ManagerName,
                AssignedWorkshopOwnerIds = @AssignedWorkshopOwnerIds,
                AssignedAreaIds = @AssignedAreaIds,
                AssignedBrandIds = @AssignedBrandIds,
                CanViewOrders = @CanViewOrders,
                CanCreateOrders = @CanCreateOrders,
                CanEditOrders = @CanEditOrders,
                CanDeleteOrders = @CanDeleteOrders,
                CanViewCustomers = @CanViewCustomers,
                CanEditCustomers = @CanEditCustomers,
                CanDeleteCustomers = @CanDeleteCustomers,
                CanViewInvoices = @CanViewInvoices,
                CanCreateInvoices = @CanCreateInvoices,
                CanEditInvoices = @CanEditInvoices,
                CanManageTeam = @CanManageTeam,
                CanViewReports = @CanViewReports,
                CanApproveOrders = @CanApproveOrders,
                CanExportData = @CanExportData,
                CanSystemSettings = @CanSystemSettings,
                CanUpdateOrderStatus = @CanUpdateOrderStatus,
                CanUpdateInventory = @CanUpdateInventory,
                CanViewStock = @CanViewStock,
                CanProcessShipments = @CanProcessShipments,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE Id = @Id";

        salesPerson.UpdatedAt = DateTime.UtcNow;
        return await connection.QuerySingleOrDefaultAsync<SalesPerson>(sql, salesPerson);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SalesPersons WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> SetStatusAsync(int id, string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE SalesPersons SET
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM SalesPersons";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetActiveCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM SalesPersons WHERE Status = 'Active'";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetCountByRoleAsync(string role)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM SalesPersons WHERE Role = @Role";
        return await connection.ExecuteScalarAsync<int>(sql, new { Role = role });
    }
}
