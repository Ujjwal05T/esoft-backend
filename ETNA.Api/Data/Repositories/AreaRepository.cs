using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Area data access using Dapper
/// </summary>
public class AreaRepository : IAreaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AreaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Area> CreateAsync(Area area)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO Areas (Name, State, Cities, CreatedAt)
            OUTPUT INSERTED.*
            VALUES (@Name, @State, @Cities, @CreatedAt)";

        var result = await connection.QuerySingleAsync<Area>(sql, area);
        return result;
    }

    public async Task<Area?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Areas WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Area>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Area>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Areas ORDER BY State, Name";
        return await connection.QueryAsync<Area>(sql);
    }

    public async Task<IEnumerable<Area>> GetByStateAsync(string state)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Areas 
            WHERE State = @State 
            ORDER BY Name";
        return await connection.QueryAsync<Area>(sql, new { State = state });
    }

    public async Task<Area?> UpdateAsync(Area area)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Areas SET
                Name = @Name,
                State = @State,
                Cities = @Cities,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE Id = @Id";

        area.UpdatedAt = DateTime.UtcNow;
        return await connection.QuerySingleOrDefaultAsync<Area>(sql, area);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Areas WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Areas";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
