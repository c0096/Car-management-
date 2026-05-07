namespace VehicleDeclarations.Entity;

public sealed class DeclarationAttachment
{
    public int Id { get; set; }

    public int DeclarationId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
