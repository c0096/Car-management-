using Microsoft.AspNetCore.Mvc;

namespace Orders.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Error()
    {
        return View();
    }
}
