using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public interface IFileStorageService
{
    Task<IReadOnlyList<DeclarationAttachment>> SaveAsync(int declarationId, IReadOnlyList<IFormFile> files);

    Task<byte[]?> ReadAsync(DeclarationAttachment attachment);

    Task DeleteAsync(IReadOnlyList<DeclarationAttachment> attachments);
}
