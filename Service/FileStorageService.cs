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
                RelativePath = $"/uploads/{declarationId}/{storedFileName}"
            });
        }

        return attachments;
    }

    public Task DeleteAsync(IReadOnlyList<DeclarationAttachment> attachments)
    {
        foreach (var attachment in attachments)
        {
            var relativePath = attachment.RelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(environment.WebRootPath, relativePath);

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
}
