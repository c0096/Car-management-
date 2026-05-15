using Microsoft.Data.SqlClient;

namespace Orders.Db;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
