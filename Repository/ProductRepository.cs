using System.Data;
using Microsoft.Data.SqlClient;
using Orders.Db;
using Orders.Entity;

namespace Orders.Repository;

public sealed class ProductRepository(ISqlConnectionFactory connectionFactory) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT
                p.Id,
                p.Name,
                p.Sku,
                p.Description,
                p.CategoryId,
                c.Name AS CategoryName,
                p.UnitPrice,
                p.StockQuantity,
                p.CreatedAt,
                p.UpdatedAt
            FROM dbo.Products p
            INNER JOIN dbo.Categories c ON c.Id = p.CategoryId
            ORDER BY p.Name;
            """;

        await using var command = new SqlCommand(sql, connection);
        var products = new List<Product>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT
                p.Id,
                p.Name,
                p.Sku,
                p.Description,
                p.CategoryId,
                c.Name AS CategoryName,
                p.UnitPrice,
                p.StockQuantity,
                p.CreatedAt,
                p.UpdatedAt
            FROM dbo.Products p
            INNER JOIN dbo.Categories c ON c.Id = p.CategoryId
            WHERE p.Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProduct(reader) : null;
    }

    public async Task<int> CreateAsync(Product product)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            INSERT INTO dbo.Products
            (
                Name,
                Sku,
                Description,
                CategoryId,
                UnitPrice,
                StockQuantity
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Name,
                @Sku,
                @Description,
                @CategoryId,
                @UnitPrice,
                @StockQuantity
            );
            """;

        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, product);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(Product product)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            UPDATE dbo.Products
            SET Name = @Name,
                Sku = @Sku,
                Description = @Description,
                CategoryId = @CategoryId,
                UnitPrice = @UnitPrice,
                StockQuantity = @StockQuantity,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = product.Id;
        AddParameters(command, product);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("DELETE FROM dbo.Products WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await command.ExecuteNonQueryAsync();
    }

    private static void AddParameters(SqlCommand command, Product product)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 160).Value = product.Name.Trim();
        command.Parameters.Add("@Sku", SqlDbType.NVarChar, 80).Value = product.Sku.Trim();
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 700).Value = string.IsNullOrWhiteSpace(product.Description) ? DBNull.Value : product.Description.Trim();
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = product.CategoryId;
        command.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = product.UnitPrice;
        command.Parameters["@UnitPrice"].Precision = 18;
        command.Parameters["@UnitPrice"].Scale = 2;
        command.Parameters.Add("@StockQuantity", SqlDbType.Int).Value = product.StockQuantity;
    }

    private static Product MapProduct(SqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Sku = reader.GetString(reader.GetOrdinal("Sku")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
            StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
