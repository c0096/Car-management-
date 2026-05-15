using Orders.Entity;

namespace Orders.Service;

public interface IOrderService
{
    Task<PagedResult<Order>> SearchAsync(SearchOptions options);

    Task<Order?> GetByIdAsync(int id);

    Task<int> CreateAsync(Order order, IReadOnlyList<IFormFile> attachmentFiles);

    Task UpdateAsync(Order order, IReadOnlyList<IFormFile> attachmentFiles, IReadOnlyList<int> removedAttachmentIds);

    Task DeleteAsync(int id);

    Task<AttachmentFile?> GetAttachmentAsync(int orderId, int attachmentId);

    Task<ReportFile> GenerateReportAsync(int id);
}
