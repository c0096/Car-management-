using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Entity;
using Orders.Service;

namespace Orders.Controllers;

[Authorize]
public sealed class CategoriesController(ICategoryService categoryService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            await categoryService.CreateAsync(category);
        }

        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            await categoryService.UpdateAsync(category);
        }

        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await categoryService.DeleteAsync(id);
        }
        catch (InvalidOperationException)
        {
            TempData["CategoryError"] = "Impossible de supprimer une catégorie utilisée par des produits.";
        }

        return RedirectToAction("Index", "Products");
    }
}
