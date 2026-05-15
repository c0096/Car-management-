using Orders.Entity;
using Orders.Repository;

namespace Orders.Service;

public sealed class OrderService(
    IOrderRepository repository,
    IFileStorageService fileStorageService,
    IPdfReportService pdfReportService) : IOrderService
{
    public Task<PagedResult<Order>> SearchAsync(SearchOptions options)
    {
        return repository.SearchAsync(options);
    }

    public Task<Order?> GetByIdAsync(int id)
    {
        return repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(Order order, IReadOnlyList<IFormFile> attachmentFiles)
    {
        var id = await repository.CreateAsync(order);
        var attachments = await fileStorageService.SaveAsync(id, attachmentFiles);
        await repository.AddAttachmentsAsync(id, attachments);
        return id;
    }

    public async Task UpdateAsync(Order order, IReadOnlyList<IFormFile> attachmentFiles, IReadOnlyList<int> removedAttachmentIds)
    {
        await repository.UpdateAsync(order);

        if (removedAttachmentIds.Count > 0)
        {
            var attachmentsToRemove = await repository.GetAttachmentsByIdsAsync(removedAttachmentIds);
            var orderAttachments = attachmentsToRemove.Where(attachment => attachment.OrderId == order.Id).ToArray();
            var orderAttachmentIds = orderAttachments.Select(attachment => attachment.Id).ToArray();
            await repository.DeleteAttachmentsAsync(orderAttachmentIds);
            await fileStorageService.DeleteAsync(orderAttachments);
        }

        var attachments = await fileStorageService.SaveAsync(order.Id, attachmentFiles);
        await repository.AddAttachmentsAsync(order.Id, attachments);
    }

    public async Task DeleteAsync(int id)
    {
        var order = await repository.GetByIdAsync(id);

        if (order is null)
        {
            return;
        }

        await repository.DeleteAsync(id);
        await fileStorageService.DeleteAsync(order.Attachments);
    }

    public async Task<AttachmentFile?> GetAttachmentAsync(int orderId, int attachmentId)
    {
        var attachment = (await repository.GetAttachmentsByIdsAsync([attachmentId])).SingleOrDefault();

        if (attachment is null || attachment.OrderId != orderId)
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
        var order = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found.");
        var pdf = pdfReportService.Generate(order);
        var orderNumber = SanitizeFileNamePart(order.OrderNumber);
        var fileName = $"order-{order.Id}-{orderNumber}.pdf";
        return new ReportFile(fileName, pdf);
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "rapport" : sanitized;
    }
}
