using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Entity;
using Orders.Service;
using Orders.ViewModels;

namespace Orders.Controllers;

[Authorize]
public sealed class ProductsController(IProductService productService, ICategoryService categoryService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await BuildIndexViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildIndexViewModel(productForm: product));
        }

        try
        {
            await productService.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Index", await BuildIndexViewModel(productForm: product));
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Categories = await categoryService.GetAllAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetAllAsync();
            return View(product);
        }

        try
        {
            await productService.UpdateAsync(product);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.Categories = await categoryService.GetAllAsync();
            return View(product);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await productService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductIndexViewModel> BuildIndexViewModel(Product? productForm = null, Category? categoryForm = null, string? categoryError = null)
    {
        return new ProductIndexViewModel
        {
            Products = await productService.GetAllAsync(),
            Categories = await categoryService.GetAllAsync(),
            ProductForm = productForm ?? new Product(),
            CategoryForm = categoryForm ?? new Category(),
            CategoryError = categoryError
        };
    }
}
