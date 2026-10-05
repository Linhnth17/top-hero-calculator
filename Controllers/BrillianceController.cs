using Microsoft.AspNetCore.Mvc;
using SpendCalc.Models;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class BrillianceController(BrillianceStore store) : Controller
{
    public IActionResult Index(int? from, int? to, int? cur, List<long?>? own)
    {
        var table = store.Table;
        int min = table.Rows.Min(r => r.Level) - 1;
        int max = table.Rows.Max(r => r.Level);
        int f = Math.Clamp(from ?? min, min, max - 1);
        int t = Math.Clamp(to ?? max, f + 1, max);

        var vm = new BrillianceVm
        {
            Table = table,
            MinLevel = min,
            MaxLevel = max,
            From = f,
            To = t,
            Cost = GearCalculator.Calculate(table, f, t),
            Cur = Math.Clamp(cur ?? min, min, max),
            Own = own ?? new()
        };

        if (vm.Own.Any(x => x.HasValue))
        {
            var g = GearCalculator.MaxReach(table, vm.Cur, vm.Own);
            vm.Reach = g;

            int next = (BrillianceVm.TierAt(g.Level) + 1) * 5 + min;
            if (next <= max)
            {
                var need = GearCalculator.Calculate(table, g.Level, next).Totals;
                vm.NextTierLevel = next;
                vm.NextTierLack = need.Select((n, i) => Math.Max(0, n - g.Remaining[i])).ToArray();
            }
        }

        return View(vm);
    }
}
