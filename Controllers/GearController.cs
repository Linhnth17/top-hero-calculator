using Microsoft.AspNetCore.Mvc;
using SpendCalc.Models;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class GearController(GearStore store) : Controller
{
    public IActionResult Index(string? tableId, int? from, int? pieces, List<long?>? own)
    {
        var table = store.Get(tableId) ?? store.All[0];
        int maxLevel = table.Rows.Max(r => r.Level);
        int p = Math.Clamp(pieces ?? 4, 1, 4);

        var vm = new GearVm
        {
            Tables = store.All.ToList(),
            Table = table,
            From = from ?? 0,
            MaxLevel = maxLevel,
            Pieces = p,
            Totals = Enumerable.Range(0, table.Resources.Count)
                .Select(i => table.Rows.Sum(r => r.Costs[i]) * p)
                .ToArray(),
            Own = own ?? new()
        };

        if (vm.Own.Any(x => x.HasValue))
            vm.Reach = GearCalculator.MaxReach(table, vm.From, vm.Own, p);

        return View(vm);
    }
}
