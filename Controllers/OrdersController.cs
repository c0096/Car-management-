using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Entity;
using Orders.Service;
using Orders.ViewModels;

namespace Orders.Controllers;

[Authorize]
public sealed class OrdersController(IOrderService service) : Controller
{
    public async Task<IActionResult> Index([FromQuery] SearchOptions options)
    {
        var results = await service.SearchAsync(options);

        return View(new OrderIndexViewModel
        {
            Results = results,
            Options = options
        });
    }

    public IActionResult Create()
    {
        return View(new Order
        {
            OrderDateTime = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Order order, List<IFormFile> attachmentFiles)
    {
        if (!ModelState.IsValid)
        {
            return View(order);
        }

        var id = await service.CreateAsync(order, attachmentFiles);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await service.GetByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Order order, List<IFormFile> attachmentFiles, List<int> removedAttachmentIds)
    {
        if (id != order.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var existing = await service.GetByIdAsync(id);
            order.Attachments = existing?.Attachments ?? [];
            return View(order);
        }

        await service.UpdateAsync(order, attachmentFiles, removedAttachmentIds);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await service.GetByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var order = await service.GetByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Report(int id)
    {
        try
        {
            var report = await service.GenerateReportAsync(id);
            return File(report.Content, "application/pdf", report.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Attachment(int id, int attachmentId)
    {
        var attachment = await service.GetAttachmentAsync(id, attachmentId);

        if (attachment is null)
        {
            return NotFound();
        }

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }
}
