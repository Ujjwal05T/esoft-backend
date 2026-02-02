using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for SalesPerson data access
/// </summary>
public interface ISalesPersonRepository
{
    // CRUD operations
    Task<SalesPerson> CreateAsync(SalesPerson salesPerson);
    Task<SalesPerson?> GetByIdAsync(int id);
    Task<SalesPerson?> GetByEmailAsync(string email);
    Task<SalesPerson?> GetByPhoneAsync(string phone);
    Task<IEnumerable<SalesPerson>> GetAllAsync();
    Task<IEnumerable<SalesPerson>> GetByRoleAsync(string role);
    Task<IEnumerable<SalesPerson>> GetByStatusAsync(string status);
    Task<IEnumerable<SalesPerson>> GetByManagerIdAsync(int managerId);
    Task<SalesPerson?> UpdateAsync(SalesPerson salesPerson);
    Task<bool> DeleteAsync(int id);
    
    // Status management
    Task<bool> SetStatusAsync(int id, string status);
    
    // Counts
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task<int> GetCountByRoleAsync(string role);
}
