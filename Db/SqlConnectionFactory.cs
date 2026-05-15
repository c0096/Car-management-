using Microsoft.Data.SqlClient;

namespace Orders.Db;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public SqlConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string DefaultConnection is missing.");
        }

        return new SqlConnection(connectionString);
    }
}
