using Microsoft.AspNetCore.Mvc;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class HeroesController(HeroStore store) : Controller
{
    public IActionResult Index() => View(store);
}
