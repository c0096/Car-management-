using Microsoft.Extensions.Options;
using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public sealed class FileStorageService(IWebHostEnvironment environment, IOptions<UploadOptions> options) : IFileStorageService
{
    public async Task<IReadOnlyList<DeclarationAttachment>> SaveAsync(int declarationId, IReadOnlyList<IFormFile> files)
    {
        var validFiles = files.Where(file => file.Length > 0).ToArray();

        if (validFiles.Length == 0)
        {
            return [];
        }

        var rootPath = ResolveRootPath();
        var declarationDirectory = Path.Combine(rootPath, declarationId.ToString());
        Directory.CreateDirectory(declarationDirectory);

        var attachments = new List<DeclarationAttachment>();

        foreach (var file in validFiles)
        {
            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(declarationDirectory, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            attachments.Add(new DeclarationAttachment
            {
                DeclarationId = declarationId,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                SizeBytes = file.Length,
                RelativePath = $"{declarationId}/{storedFileName}"
            });
        }

        return attachments;
    }

    public async Task<byte[]?> ReadAsync(DeclarationAttachment attachment)
    {
        var fullPath = ResolveFilePath(attachment);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteAsync(IReadOnlyList<DeclarationAttachment> attachments)
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

    private string ResolveFilePath(DeclarationAttachment attachment)
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
