using Microsoft.Extensions.Options;
using Orders.Entity;

namespace Orders.Service;

public sealed class FileStorageService(IWebHostEnvironment environment, IOptions<UploadOptions> options) : IFileStorageService
{
    public async Task<IReadOnlyList<OrderAttachment>> SaveAsync(int orderId, IReadOnlyList<IFormFile> files)
    {
        var validFiles = files.Where(file => file.Length > 0).ToArray();

        if (validFiles.Length == 0)
        {
            return [];
        }

        var rootPath = ResolveRootPath();
        var orderDirectory = Path.Combine(rootPath, orderId.ToString());
        Directory.CreateDirectory(orderDirectory);

        var attachments = new List<OrderAttachment>();

        foreach (var file in validFiles)
        {
            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(orderDirectory, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            attachments.Add(new OrderAttachment
            {
                OrderId = orderId,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                SizeBytes = file.Length,
                RelativePath = $"{orderId}/{storedFileName}"
            });
        }

        return attachments;
    }

    public async Task<byte[]?> ReadAsync(OrderAttachment attachment)
    {
        var fullPath = ResolveFilePath(attachment);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteAsync(IReadOnlyList<OrderAttachment> attachments)
    {
        foreach (var attachment in attachments)
        {
            var fullPath = ResolveFilePath(attachment);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        return Task.CompletedTask;
    }

    private string ResolveRootPath()
    {
        var configuredPath = options.Value.RootPath;

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(environment.ContentRootPath, configuredPath);
    }

    private string ResolveFilePath(OrderAttachment attachment)
    {
        if (attachment.RelativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            var publicRelativePath = attachment.RelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(environment.WebRootPath, publicRelativePath);
        }

        var safeRelativePath = attachment.RelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(ResolveRootPath(), safeRelativePath);
    }
}
