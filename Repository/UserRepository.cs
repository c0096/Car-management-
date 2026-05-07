using System.Data;
using Microsoft.Data.SqlClient;
using VehicleDeclarations.Db;
using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Repository;

public sealed class UserRepository(ISqlConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT
                Id,
                Email,
                PasswordHash,
                CreatedAt
            FROM dbo.AppUsers
            WHERE NormalizedEmail = @NormalizedEmail;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@NormalizedEmail", SqlDbType.NVarChar, 256).Value = NormalizeEmail(email);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<bool> AnyAsync()
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("SELECT COUNT(1) FROM dbo.AppUsers;", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    public async Task<int> CreateAsync(AppUser user)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            INSERT INTO dbo.AppUsers
            (
                Email,
                NormalizedEmail,
                PasswordHash
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Email,
                @NormalizedEmail,
                @PasswordHash
            );
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = user.Email.Trim();
        command.Parameters.Add("@NormalizedEmail", SqlDbType.NVarChar, 256).Value = NormalizeEmail(user.Email);
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.PasswordHash;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static AppUser MapUser(SqlDataReader reader)
    {
        return new AppUser
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
