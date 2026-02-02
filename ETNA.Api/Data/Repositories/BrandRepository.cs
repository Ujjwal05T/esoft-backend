using Dapper;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Brand data access using Dapper
/// </summary>
public class BrandRepository : IBrandRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BrandRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Brand> CreateAsync(Brand brand)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO Brands (Name, LogoUrl, IsActive, CreatedAt)
            OUTPUT INSERTED.*
            VALUES (@Name, @LogoUrl, @IsActive, @CreatedAt)";

        var result = await connection.QuerySingleAsync<Brand>(sql, brand);
        return result;
    }

    public async Task<Brand?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Brands WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Brand>(sql, new { Id = id });
    }

    public async Task<Brand?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Brands WHERE Name = @Name";
        return await connection.QuerySingleOrDefaultAsync<Brand>(sql, new { Name = name });
    }

    public async Task<IEnumerable<Brand>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Brands ORDER BY Name";
        return await connection.QueryAsync<Brand>(sql);
    }

    public async Task<IEnumerable<Brand>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Brands WHERE IsActive = 1 ORDER BY Name";
        return await connection.QueryAsync<Brand>(sql);
    }

    public async Task<Brand?> UpdateAsync(Brand brand)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Brands SET
                Name = @Name,
                LogoUrl = @LogoUrl,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE Id = @Id";

        brand.UpdatedAt = DateTime.UtcNow;
        return await connection.QuerySingleOrDefaultAsync<Brand>(sql, brand);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Brands WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Brands";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
