using VehicleDeclarations.Entity;
using VehicleDeclarations.Repository;

namespace VehicleDeclarations.Service;

public sealed class VehicleDeclarationService(
    IVehicleDeclarationRepository repository,
    IFileStorageService fileStorageService,
    IPdfReportService pdfReportService) : IVehicleDeclarationService
{
    public Task<PagedResult<VehicleSaleDeclaration>> SearchAsync(SearchOptions options)
    {
        return repository.SearchAsync(options);
    }

    public Task<VehicleSaleDeclaration?> GetByIdAsync(int id)
    {
        return repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(VehicleSaleDeclaration declaration, IReadOnlyList<IFormFile> attachmentFiles)
    {
        var id = await repository.CreateAsync(declaration);
        var attachments = await fileStorageService.SaveAsync(id, attachmentFiles);
        await repository.AddAttachmentsAsync(id, attachments);
        return id;
    }

    public async Task UpdateAsync(VehicleSaleDeclaration declaration, IReadOnlyList<IFormFile> attachmentFiles, IReadOnlyList<int> removedAttachmentIds)
    {
        await repository.UpdateAsync(declaration);

        if (removedAttachmentIds.Count > 0)
        {
            var attachmentsToRemove = await repository.GetAttachmentsByIdsAsync(removedAttachmentIds);
            var declarationAttachments = attachmentsToRemove.Where(attachment => attachment.DeclarationId == declaration.Id).ToArray();
            var declarationAttachmentIds = declarationAttachments.Select(attachment => attachment.Id).ToArray();
            await repository.DeleteAttachmentsAsync(declarationAttachmentIds);
            await fileStorageService.DeleteAsync(declarationAttachments);
        }

        var attachments = await fileStorageService.SaveAsync(declaration.Id, attachmentFiles);
        await repository.AddAttachmentsAsync(declaration.Id, attachments);
    }

    public async Task DeleteAsync(int id)
    {
        var declaration = await repository.GetByIdAsync(id);

        if (declaration is null)
        {
            return;
        }

        await repository.DeleteAsync(id);
        await fileStorageService.DeleteAsync(declaration.Attachments);
    }

    public async Task<AttachmentFile?> GetAttachmentAsync(int declarationId, int attachmentId)
    {
        var attachment = (await repository.GetAttachmentsByIdsAsync([attachmentId])).SingleOrDefault();

        if (attachment is null || attachment.DeclarationId != declarationId)
        {
            return null;
        }

        var content = await fileStorageService.ReadAsync(attachment);

        if (content is null)
        {
            return null;
        }

        return new AttachmentFile(attachment.OriginalFileName, attachment.ContentType, content);
    }

    public async Task<ReportFile> GenerateReportAsync(int id)
    {
        var declaration = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Declaration not found.");
        var pdf = pdfReportService.Generate(declaration);
        var orderNumber = SanitizeFileNamePart(declaration.OrderNumber);
        var fileName = $"declaration-{declaration.Id}-{orderNumber}.pdf";
        return new ReportFile(fileName, pdf);
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "rapport" : sanitized;
    }
}
