using Microsoft.AspNetCore.Mvc;
using SpendCalc.Models;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class HomeController(EventStore store) : Controller
{
    [HttpGet]
    public IActionResult Index(string? eventId)
    {
        var ev = store.Get(eventId) ?? store.All[0];
        return View(new PlannerVm
        {
            EventId = ev.Id,
            Items = ev.Items.Select(i => new ItemInput { Name = i.Name, Cost = i.Cost, Points = i.Points, MaxTimes = i.MaxTimes }).ToList(),
            Events = store.All.ToList()
        });
    }

    [HttpPost]
    public IActionResult Index(PlannerVm vm)
    {
        vm.Events = store.All.ToList();
        vm.Items ??= new();
        ModelState.Clear();   // để view hiển thị đúng giá trị trong vm
        try { vm.Result = MinCostSolver.Solve(vm.Items, vm.Target); }
        catch (ArgumentException ex) { vm.Error = ex.Message; }
        return View(vm);
    }
}
