using Microsoft.AspNetCore.Mvc;

namespace VehicleDeclarations.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Error()
    {
        return View();
    }
}
