using System.Data;
using Microsoft.Data.SqlClient;
using VehicleDeclarations.Db;
using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Repository;

public sealed class VehicleDeclarationRepository(ISqlConnectionFactory connectionFactory) : IVehicleDeclarationRepository
{
    private static readonly IReadOnlyDictionary<string, string> SortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["date"] = "DeclarationDateTime",
        ["writer"] = "WriterName",
        ["seller"] = "SellerName",
        ["buyer"] = "BuyerName",
        ["vehicle"] = "VehicleBrand",
        ["city"] = "City",
        ["order"] = "OrderNumber",
        ["created"] = "CreatedAt"
    };

    public async Task<PagedResult<VehicleSaleDeclaration>> SearchAsync(SearchOptions options)
    {
        NormalizeOptions(options);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var whereClause = BuildWhereClause(options.Search);
        var sortColumn = SortColumns.TryGetValue(options.Sort, out var mappedSort) ? mappedSort : SortColumns["date"];
        var direction = string.Equals(options.Direction, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

        var countSql = $"SELECT COUNT(1) FROM dbo.VehicleSaleDeclarations d {whereClause}";
        await using var countCommand = new SqlCommand(countSql, connection);
        AddSearchParameter(countCommand, options.Search);
        var totalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        var sql = $"""
            SELECT
                d.Id,
                d.WriterName,
                d.AuthorizationNumber,
                d.WriterPhone,
                d.City,
                d.DeclarationDateTime,
                d.SellerName,
                d.SellerAddress,
                d.SellerCin,
                d.SellerPhone,
                d.SoldItemDescription,
                d.OrderNumber,
                d.VehicleType,
                d.VehicleBrand,
                d.ChassisNumber,
                d.BuyerName,
                d.BuyerAddress,
                d.BuyerCin,
                d.BuyerPhone,
                d.PropertyTitle,
                d.Observation,
                d.SellerSignature,
                d.ManagerSignature,
                d.BuyerSignature,
                d.CreatedAt,
                d.UpdatedAt
            FROM dbo.VehicleSaleDeclarations d
            {whereClause}
            ORDER BY d.{sortColumn} {direction}, d.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        await using var command = new SqlCommand(sql, connection);
        AddSearchParameter(command, options.Search);
        command.Parameters.Add("@Offset", SqlDbType.Int).Value = options.Offset;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = options.PageSize;

        var declarations = new List<VehicleSaleDeclaration>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            declarations.Add(MapDeclaration(reader));
        }

        return new PagedResult<VehicleSaleDeclaration>
        {
            Items = declarations,
            TotalItems = totalItems,
            Page = options.Page,
            PageSize = options.PageSize
        };
    }

    public async Task<VehicleSaleDeclaration?> GetByIdAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT
                Id,
                WriterName,
                AuthorizationNumber,
                WriterPhone,
                City,
                DeclarationDateTime,
                SellerName,
                SellerAddress,
                SellerCin,
                SellerPhone,
                SoldItemDescription,
                OrderNumber,
                VehicleType,
                VehicleBrand,
                ChassisNumber,
                BuyerName,
                BuyerAddress,
                BuyerCin,
                BuyerPhone,
                PropertyTitle,
                Observation,
                SellerSignature,
                ManagerSignature,
                BuyerSignature,
                CreatedAt,
                UpdatedAt
            FROM dbo.VehicleSaleDeclarations
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        VehicleSaleDeclaration? declaration = null;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                declaration = MapDeclaration(reader);
            }
        }

        if (declaration is null)
        {
            return null;
        }

        declaration.Attachments = await GetAttachmentsForDeclarationAsync(connection, id);
        return declaration;
    }

    public async Task<int> CreateAsync(VehicleSaleDeclaration declaration)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            INSERT INTO dbo.VehicleSaleDeclarations
            (
                WriterName,
                AuthorizationNumber,
                WriterPhone,
                City,
                DeclarationDateTime,
                SellerName,
                SellerAddress,
                SellerCin,
                SellerPhone,
                SoldItemDescription,
                OrderNumber,
                VehicleType,
                VehicleBrand,
                ChassisNumber,
                BuyerName,
                BuyerAddress,
                BuyerCin,
                BuyerPhone,
                PropertyTitle,
                Observation,
                SellerSignature,
                ManagerSignature,
                BuyerSignature
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @WriterName,
                @AuthorizationNumber,
                @WriterPhone,
                @City,
                @DeclarationDateTime,
                @SellerName,
                @SellerAddress,
                @SellerCin,
                @SellerPhone,
                @SoldItemDescription,
                @OrderNumber,
                @VehicleType,
                @VehicleBrand,
                @ChassisNumber,
                @BuyerName,
                @BuyerAddress,
                @BuyerCin,
                @BuyerPhone,
                @PropertyTitle,
                @Observation,
                @SellerSignature,
                @ManagerSignature,
                @BuyerSignature
            );
            """;

        await using var command = new SqlCommand(sql, connection);
        AddDeclarationParameters(command, declaration);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(VehicleSaleDeclaration declaration)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            UPDATE dbo.VehicleSaleDeclarations
            SET
                WriterName = @WriterName,
                AuthorizationNumber = @AuthorizationNumber,
                WriterPhone = @WriterPhone,
                City = @City,
                DeclarationDateTime = @DeclarationDateTime,
                SellerName = @SellerName,
                SellerAddress = @SellerAddress,
                SellerCin = @SellerCin,
                SellerPhone = @SellerPhone,
                SoldItemDescription = @SoldItemDescription,
                OrderNumber = @OrderNumber,
                VehicleType = @VehicleType,
                VehicleBrand = @VehicleBrand,
                ChassisNumber = @ChassisNumber,
                BuyerName = @BuyerName,
                BuyerAddress = @BuyerAddress,
                BuyerCin = @BuyerCin,
                BuyerPhone = @BuyerPhone,
                PropertyTitle = @PropertyTitle,
                Observation = @Observation,
                SellerSignature = @SellerSignature,
                ManagerSignature = @ManagerSignature,
                BuyerSignature = @BuyerSignature,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = declaration.Id;
        AddDeclarationParameters(command, declaration);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("DELETE FROM dbo.VehicleSaleDeclarations WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAttachmentsAsync(int declarationId, IReadOnlyList<DeclarationAttachment> attachments)
    {
        if (attachments.Count == 0)
        {
            return;
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        foreach (var attachment in attachments)
        {
            var sql = """
                INSERT INTO dbo.DeclarationAttachments
                (
                    DeclarationId,
                    OriginalFileName,
                    StoredFileName,
                    ContentType,
                    SizeBytes,
                    RelativePath
                )
                VALUES
                (
                    @DeclarationId,
                    @OriginalFileName,
                    @StoredFileName,
                    @ContentType,
                    @SizeBytes,
                    @RelativePath
                );
                """;

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@DeclarationId", SqlDbType.Int).Value = declarationId;
            AddAttachmentParameters(command, attachment);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IReadOnlyList<DeclarationAttachment>> GetAttachmentsByIdsAsync(IReadOnlyList<int> attachmentIds)
    {
        if (attachmentIds.Count == 0)
        {
            return [];
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var parameterNames = attachmentIds.Select((_, index) => $"@Id{index}").ToArray();
        var sql = $"""
            SELECT
                Id,
                DeclarationId,
                OriginalFileName,
                StoredFileName,
                ContentType,
                SizeBytes,
                RelativePath,
                UploadedAt
            FROM dbo.DeclarationAttachments
            WHERE Id IN ({string.Join(", ", parameterNames)});
            """;

        await using var command = new SqlCommand(sql, connection);

        for (var index = 0; index < attachmentIds.Count; index++)
        {
            command.Parameters.Add(parameterNames[index], SqlDbType.Int).Value = attachmentIds[index];
        }

        var attachments = new List<DeclarationAttachment>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            attachments.Add(MapAttachment(reader));
        }

        return attachments;
    }

    public async Task DeleteAttachmentsAsync(IReadOnlyList<int> attachmentIds)
    {
        if (attachmentIds.Count == 0)
        {
            return;
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var parameterNames = attachmentIds.Select((_, index) => $"@Id{index}").ToArray();
        var sql = $"DELETE FROM dbo.DeclarationAttachments WHERE Id IN ({string.Join(", ", parameterNames)});";
        await using var command = new SqlCommand(sql, connection);

        for (var index = 0; index < attachmentIds.Count; index++)
        {
            command.Parameters.Add(parameterNames[index], SqlDbType.Int).Value = attachmentIds[index];
        }

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<List<DeclarationAttachment>> GetAttachmentsForDeclarationAsync(SqlConnection connection, int declarationId)
    {
        var sql = """
            SELECT
                Id,
                DeclarationId,
                OriginalFileName,
                StoredFileName,
                ContentType,
                SizeBytes,
                RelativePath,
                UploadedAt
            FROM dbo.DeclarationAttachments
            WHERE DeclarationId = @DeclarationId
            ORDER BY UploadedAt DESC, Id DESC;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DeclarationId", SqlDbType.Int).Value = declarationId;

        var attachments = new List<DeclarationAttachment>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            attachments.Add(MapAttachment(reader));
        }

        return attachments;
    }

    private static string BuildWhereClause(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return string.Empty;
        }

        return """
            WHERE
                d.WriterName LIKE @Search
                OR d.AuthorizationNumber LIKE @Search
                OR d.WriterPhone LIKE @Search
                OR d.City LIKE @Search
                OR CONVERT(NVARCHAR(30), d.DeclarationDateTime, 120) LIKE @Search
                OR d.SellerName LIKE @Search
                OR d.SellerAddress LIKE @Search
                OR d.SellerCin LIKE @Search
                OR d.SellerPhone LIKE @Search
                OR d.SoldItemDescription LIKE @Search
                OR d.OrderNumber LIKE @Search
                OR d.VehicleType LIKE @Search
                OR d.VehicleBrand LIKE @Search
                OR d.ChassisNumber LIKE @Search
                OR d.BuyerName LIKE @Search
                OR d.BuyerAddress LIKE @Search
                OR d.BuyerCin LIKE @Search
                OR d.BuyerPhone LIKE @Search
                OR d.PropertyTitle LIKE @Search
                OR d.Observation LIKE @Search
                OR d.SellerSignature LIKE @Search
                OR d.ManagerSignature LIKE @Search
                OR d.BuyerSignature LIKE @Search
                OR EXISTS
                (
                    SELECT 1
                    FROM dbo.DeclarationAttachments a
                    WHERE a.DeclarationId = d.Id
                        AND a.OriginalFileName LIKE @Search
                )
            """;
    }

    private static void AddSearchParameter(SqlCommand command, string? search)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.Add("@Search", SqlDbType.NVarChar, 1100).Value = $"%{search.Trim()}%";
        }
    }

    private static void AddDeclarationParameters(SqlCommand command, VehicleSaleDeclaration declaration)
    {
        command.Parameters.Add("@WriterName", SqlDbType.NVarChar, 150).Value = declaration.WriterName.Trim();
        command.Parameters.Add("@AuthorizationNumber", SqlDbType.NVarChar, 80).Value = declaration.AuthorizationNumber.Trim();
        command.Parameters.Add("@WriterPhone", SqlDbType.NVarChar, 40).Value = declaration.WriterPhone.Trim();
        command.Parameters.Add("@City", SqlDbType.NVarChar, 120).Value = declaration.City.Trim();
        command.Parameters.Add("@DeclarationDateTime", SqlDbType.DateTime2).Value = declaration.DeclarationDateTime;
        command.Parameters.Add("@SellerName", SqlDbType.NVarChar, 150).Value = declaration.SellerName.Trim();
        command.Parameters.Add("@SellerAddress", SqlDbType.NVarChar, 250).Value = declaration.SellerAddress.Trim();
        command.Parameters.Add("@SellerCin", SqlDbType.NVarChar, 80).Value = declaration.SellerCin.Trim();
        command.Parameters.Add("@SellerPhone", SqlDbType.NVarChar, 40).Value = declaration.SellerPhone.Trim();
        command.Parameters.Add("@SoldItemDescription", SqlDbType.NVarChar, 250).Value = declaration.SoldItemDescription.Trim();
        command.Parameters.Add("@OrderNumber", SqlDbType.NVarChar, 80).Value = declaration.OrderNumber.Trim();
        command.Parameters.Add("@VehicleType", SqlDbType.NVarChar, 120).Value = declaration.VehicleType.Trim();
        command.Parameters.Add("@VehicleBrand", SqlDbType.NVarChar, 120).Value = declaration.VehicleBrand.Trim();
        command.Parameters.Add("@ChassisNumber", SqlDbType.NVarChar, 120).Value = declaration.ChassisNumber.Trim();
        command.Parameters.Add("@BuyerName", SqlDbType.NVarChar, 150).Value = declaration.BuyerName.Trim();
        command.Parameters.Add("@BuyerAddress", SqlDbType.NVarChar, 250).Value = declaration.BuyerAddress.Trim();
        command.Parameters.Add("@BuyerCin", SqlDbType.NVarChar, 80).Value = declaration.BuyerCin.Trim();
        command.Parameters.Add("@BuyerPhone", SqlDbType.NVarChar, 40).Value = declaration.BuyerPhone.Trim();
        command.Parameters.Add("@PropertyTitle", SqlDbType.NVarChar, 180).Value = declaration.PropertyTitle.Trim();
        command.Parameters.Add("@Observation", SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(declaration.Observation) ? DBNull.Value : declaration.Observation.Trim();
        command.Parameters.Add("@SellerSignature", SqlDbType.NVarChar, 150).Value = declaration.SellerSignature.Trim();
        command.Parameters.Add("@ManagerSignature", SqlDbType.NVarChar, 150).Value = declaration.ManagerSignature.Trim();
        command.Parameters.Add("@BuyerSignature", SqlDbType.NVarChar, 150).Value = declaration.BuyerSignature.Trim();
    }

    private static void AddAttachmentParameters(SqlCommand command, DeclarationAttachment attachment)
    {
        command.Parameters.Add("@OriginalFileName", SqlDbType.NVarChar, 260).Value = attachment.OriginalFileName;
        command.Parameters.Add("@StoredFileName", SqlDbType.NVarChar, 260).Value = attachment.StoredFileName;
        command.Parameters.Add("@ContentType", SqlDbType.NVarChar, 120).Value = attachment.ContentType;
        command.Parameters.Add("@SizeBytes", SqlDbType.BigInt).Value = attachment.SizeBytes;
        command.Parameters.Add("@RelativePath", SqlDbType.NVarChar, 500).Value = attachment.RelativePath;
    }

    private static VehicleSaleDeclaration MapDeclaration(SqlDataReader reader)
    {
        return new VehicleSaleDeclaration
        {
            Id = GetInt32(reader, "Id"),
            WriterName = GetString(reader, "WriterName"),
            AuthorizationNumber = GetString(reader, "AuthorizationNumber"),
            WriterPhone = GetString(reader, "WriterPhone"),
            City = GetString(reader, "City"),
            DeclarationDateTime = GetDateTime(reader, "DeclarationDateTime"),
            SellerName = GetString(reader, "SellerName"),
            SellerAddress = GetString(reader, "SellerAddress"),
            SellerCin = GetString(reader, "SellerCin"),
            SellerPhone = GetString(reader, "SellerPhone"),
            SoldItemDescription = GetString(reader, "SoldItemDescription"),
            OrderNumber = GetString(reader, "OrderNumber"),
            VehicleType = GetString(reader, "VehicleType"),
            VehicleBrand = GetString(reader, "VehicleBrand"),
            ChassisNumber = GetString(reader, "ChassisNumber"),
            BuyerName = GetString(reader, "BuyerName"),
            BuyerAddress = GetString(reader, "BuyerAddress"),
            BuyerCin = GetString(reader, "BuyerCin"),
            BuyerPhone = GetString(reader, "BuyerPhone"),
            PropertyTitle = GetString(reader, "PropertyTitle"),
            Observation = IsDbNull(reader, "Observation") ? null : GetString(reader, "Observation"),
            SellerSignature = GetString(reader, "SellerSignature"),
            ManagerSignature = GetString(reader, "ManagerSignature"),
            BuyerSignature = GetString(reader, "BuyerSignature"),
            CreatedAt = GetDateTime(reader, "CreatedAt"),
            UpdatedAt = GetDateTime(reader, "UpdatedAt")
        };
    }

    private static DeclarationAttachment MapAttachment(SqlDataReader reader)
    {
        return new DeclarationAttachment
        {
            Id = GetInt32(reader, "Id"),
            DeclarationId = GetInt32(reader, "DeclarationId"),
            OriginalFileName = GetString(reader, "OriginalFileName"),
            StoredFileName = GetString(reader, "StoredFileName"),
            ContentType = GetString(reader, "ContentType"),
            SizeBytes = GetInt64(reader, "SizeBytes"),
            RelativePath = GetString(reader, "RelativePath"),
            UploadedAt = GetDateTime(reader, "UploadedAt")
        };
    }

    private static int GetInt32(SqlDataReader reader, string name)
    {
        return reader.GetInt32(reader.GetOrdinal(name));
    }

    private static long GetInt64(SqlDataReader reader, string name)
    {
        return reader.GetInt64(reader.GetOrdinal(name));
    }

    private static string GetString(SqlDataReader reader, string name)
    {
        return reader.GetString(reader.GetOrdinal(name));
    }

    private static DateTime GetDateTime(SqlDataReader reader, string name)
    {
        return reader.GetDateTime(reader.GetOrdinal(name));
    }

    private static bool IsDbNull(SqlDataReader reader, string name)
    {
        return reader.IsDBNull(reader.GetOrdinal(name));
    }

    private static void NormalizeOptions(SearchOptions options)
    {
        options.Page = Math.Max(options.Page, 1);
        options.PageSize = options.PageSize is < 5 or > 100 ? 10 : options.PageSize;
        options.Sort = SortColumns.ContainsKey(options.Sort) ? options.Sort : "date";
        options.Direction = string.Equals(options.Direction, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
    }
}
