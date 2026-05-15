using Orders.Entity;

namespace Orders.Service;

public interface IFileStorageService
{
    Task<IReadOnlyList<OrderAttachment>> SaveAsync(int orderId, IReadOnlyList<IFormFile> files);

    Task<byte[]?> ReadAsync(OrderAttachment attachment);

    Task DeleteAsync(IReadOnlyList<OrderAttachment> attachments);
}
