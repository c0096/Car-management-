using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Repository;

public interface IVehicleDeclarationRepository
{
    Task<PagedResult<VehicleSaleDeclaration>> SearchAsync(SearchOptions options);

    Task<VehicleSaleDeclaration?> GetByIdAsync(int id);

    Task<int> CreateAsync(VehicleSaleDeclaration declaration);

    Task UpdateAsync(VehicleSaleDeclaration declaration);

    Task DeleteAsync(int id);

    Task AddAttachmentsAsync(int declarationId, IReadOnlyList<DeclarationAttachment> attachments);

    Task<IReadOnlyList<DeclarationAttachment>> GetAttachmentsByIdsAsync(IReadOnlyList<int> attachmentIds);

    Task DeleteAttachmentsAsync(IReadOnlyList<int> attachmentIds);
}
