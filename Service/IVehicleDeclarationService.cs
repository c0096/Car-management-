using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public interface IVehicleDeclarationService
{
    Task<PagedResult<VehicleSaleDeclaration>> SearchAsync(SearchOptions options);

    Task<VehicleSaleDeclaration?> GetByIdAsync(int id);

    Task<int> CreateAsync(VehicleSaleDeclaration declaration, IReadOnlyList<IFormFile> attachmentFiles);

    Task UpdateAsync(VehicleSaleDeclaration declaration, IReadOnlyList<IFormFile> attachmentFiles, IReadOnlyList<int> removedAttachmentIds);

    Task DeleteAsync(int id);

    Task<ReportFile> GenerateReportAsync(int id);
}
