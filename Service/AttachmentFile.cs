namespace Orders.Service;

public sealed record AttachmentFile(string FileName, string ContentType, byte[] Content);
