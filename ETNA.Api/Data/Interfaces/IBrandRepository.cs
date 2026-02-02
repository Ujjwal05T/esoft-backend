using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for Brand data access
/// </summary>
public interface IBrandRepository
{
    Task<Brand> CreateAsync(Brand brand);
    Task<Brand?> GetByIdAsync(int id);
    Task<Brand?> GetByNameAsync(string name);
    Task<IEnumerable<Brand>> GetAllAsync();
    Task<IEnumerable<Brand>> GetActiveAsync();
    Task<Brand?> UpdateAsync(Brand brand);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}
