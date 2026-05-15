using System.Data;
using Microsoft.Data.SqlClient;
using Orders.Db;
using Orders.Entity;

namespace Orders.Repository;

public sealed class CategoryRepository(ISqlConnectionFactory connectionFactory) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT Id, Name, Description, CreatedAt, UpdatedAt
            FROM dbo.Categories
            ORDER BY Name;
            """;

        await using var command = new SqlCommand(sql, connection);
        var categories = new List<Category>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT Id, Name, Description, CreatedAt, UpdatedAt
            FROM dbo.Categories
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategory(reader) : null;
    }

    public async Task<int> CreateAsync(Category category)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            INSERT INTO dbo.Categories (Name, Description)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description);
            """;

        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, category);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(Category category)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            UPDATE dbo.Categories
            SET Name = @Name,
                Description = @Description,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = category.Id;
        AddParameters(command, category);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("DELETE FROM dbo.Categories WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("SELECT COUNT(1) FROM dbo.Products WHERE CategoryId = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    private static void AddParameters(SqlCommand command, Category category)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = category.Name.Trim();
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = string.IsNullOrWhiteSpace(category.Description) ? DBNull.Value : category.Description.Trim();
    }

    private static Category MapCategory(SqlDataReader reader)
    {
        return new Category
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
