using Microsoft.AspNetCore.Mvc;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class HomeController(HeroStore heroes, TrialStore trials) : Controller
{
    public IActionResult Index()
    {
        ViewData["Heroes"] = heroes.Data.Heroes.Count;
        ViewData["Sets"] = heroes.Data.Sets.Count;
        ViewData["Sources"] = trials.All[0].Sources.Count;
        return View();
    }
}
