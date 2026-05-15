using Orders.Entity;

namespace Orders.Repository;

public interface IOrderRepository
{
    Task<PagedResult<Order>> SearchAsync(SearchOptions options);

    Task<Order?> GetByIdAsync(int id);

    Task<int> CreateAsync(Order order);

    Task UpdateAsync(Order order);

    Task DeleteAsync(int id);

    Task AddAttachmentsAsync(int orderId, IReadOnlyList<OrderAttachment> attachments);

    Task<IReadOnlyList<OrderAttachment>> GetAttachmentsByIdsAsync(IReadOnlyList<int> attachmentIds);

    Task DeleteAttachmentsAsync(IReadOnlyList<int> attachmentIds);
}
