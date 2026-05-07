using Microsoft.Data.SqlClient;

namespace VehicleDeclarations.Db;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
