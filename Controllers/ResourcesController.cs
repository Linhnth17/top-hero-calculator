using Microsoft.AspNetCore.Mvc;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class ResourcesController(ResourceCatalog catalog) : Controller
{
    public IActionResult Index() => View(catalog.All);
}
