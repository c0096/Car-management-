using Microsoft.AspNetCore.Mvc;
using VehicleDeclarations.Entity;
using VehicleDeclarations.Service;
using VehicleDeclarations.ViewModels;

namespace VehicleDeclarations.Controllers;

public sealed class VehicleDeclarationsController(IVehicleDeclarationService service) : Controller
{
    public async Task<IActionResult> Index([FromQuery] SearchOptions options)
    {
        var results = await service.SearchAsync(options);

        return View(new DeclarationIndexViewModel
        {
            Results = results,
            Options = options
        });
    }

    public IActionResult Create()
    {
        return View(new VehicleSaleDeclaration
        {
            DeclarationDateTime = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleSaleDeclaration declaration, List<IFormFile> attachmentFiles)
    {
        if (!ModelState.IsValid)
        {
            return View(declaration);
        }

        var id = await service.CreateAsync(declaration, attachmentFiles);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var declaration = await service.GetByIdAsync(id);

        if (declaration is null)
        {
            return NotFound();
        }

        return View(declaration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleSaleDeclaration declaration, List<IFormFile> attachmentFiles, List<int> removedAttachmentIds)
    {
        if (id != declaration.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var existing = await service.GetByIdAsync(id);
            declaration.Attachments = existing?.Attachments ?? [];
            return View(declaration);
        }

        await service.UpdateAsync(declaration, attachmentFiles, removedAttachmentIds);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var declaration = await service.GetByIdAsync(id);

        if (declaration is null)
        {
            return NotFound();
        }

        return View(declaration);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var declaration = await service.GetByIdAsync(id);

        if (declaration is null)
        {
            return NotFound();
        }

        return View(declaration);
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
}
