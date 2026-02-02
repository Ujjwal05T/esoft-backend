using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

/// <summary>
/// Repository interface for Area data access
/// </summary>
public interface IAreaRepository
{
    Task<Area> CreateAsync(Area area);
    Task<Area?> GetByIdAsync(int id);
    Task<IEnumerable<Area>> GetAllAsync();
    Task<IEnumerable<Area>> GetByStateAsync(string state);
    Task<Area?> UpdateAsync(Area area);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}
