namespace Orders.Entity;

public sealed class OrderAttachment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
