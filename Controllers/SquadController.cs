using Microsoft.AspNetCore.Mvc;
using SpendCalc.Models;
using SpendCalc.Services;

namespace SpendCalc.Controllers;

public class SquadController(TrialStore store, GearStore gear) : Controller
{
    public IActionResult Index(List<int?>? levels, List<int?>? wings, List<long?>? own, List<long?>? extra,
        List<int?>? sets, int? tab, List<int?>? targets)
    {
        var ev = store.All[0];
        var table = gear.Chain("Cấp + Sao", "hero-gear", "hero-gear-promotion");
        int count = SquadVm.Heroes * SquadVm.Slots;
        bool first = levels is not { Count: > 0 };

        // Lần đầu mở trang: cả 16 món ở cấp 0, chưa có sao
        var lv = Enumerable.Range(0, count)
            .Select(i => first ? 0 : i < levels!.Count && levels[i] is int v ? Math.Clamp(v, 0, SquadVm.MaxLevel) : (int?)null)
            .ToList();
        // Chưa đủ cấp 40 thì chưa có sao
        var wg = Enumerable.Range(0, count)
            .Select(i => lv[i] == SquadVm.MaxLevel && wings != null && i < wings.Count && wings[i] is int w
                ? Math.Clamp(w, 0, SquadVm.MaxWings) : 0)
            .ToList();

        // Target chỉ có nghĩa khi cao hơn mức hiện tại
        var steps = lv.Select((l, i) => l is int v ? SquadVm.ToStep(v, wg[i]) : (int?)null).ToList();
        int maxStep = SquadVm.MaxLevel + SquadVm.MaxWings;
        var tg = Enumerable.Range(0, count)
            .Select(i =>
            {
                if (steps[i] is not int cur || targets == null || i >= targets.Count || targets[i] is not int t) return (int?)null;
                t = Math.Clamp(t, 0, maxStep);
                return t > cur ? t : null;
            })
            .ToList();

        var gearIds = gear.All.SelectMany(t => t.ResourceIds).ToHashSet();
        var vm = new SquadVm
        {
            Event = ev,
            Table = table,
            Levels = lv,
            Wings = wg,
            Targets = tg,
            Own = own ?? new(),
            ExtraSources = ev.Sources.Where(s => !gearIds.Contains(s.ResourceId)).ToList(),
            Extra = extra ?? new(),
            Sets = Enumerable.Range(0, SquadVm.Heroes)
                .Select(h => sets != null && h < sets.Count && sets[h] is int s ? Math.Clamp(s, 1, SquadVm.GearSets) : 1)
                .ToList(),
            Tab = Math.Clamp(tab ?? 0, 0, SquadVm.Heroes - 1)
        };

        if (vm.Own.Any(x => x.HasValue) && lv.Any(x => x.HasValue))
        {
            // Ưu tiên nâng các món có target trước, được tới đâu hay tới đó
            var phase = SquadPlanner.ApplyTargets(table, steps, tg, vm.Own);
            if (tg.Any(x => x.HasValue))
            {
                vm.TargetNeed = SquadPlanner.TargetCost(table, steps, tg);
                vm.TargetReached = phase.Levels;
                var more = SquadPlanner.TargetCost(table, phase.Levels, tg);
                var lack = more.Select((x, i) => Math.Max(0, x - phase.Remaining[i])).ToArray();
                if (lack.Any(x => x > 0)) vm.TargetLack = lack;
            }

            vm.Plans = SquadPlanner.Plan(ev, table, steps, phase, id =>
            {
                int i = vm.ExtraSources.FindIndex(s => s.ResourceId == id);
                return i >= 0 && i < vm.Extra.Count ? vm.Extra[i] ?? 0 : 0;
            });
            vm.Best = vm.Plans[0];
        }

        return View(vm);
    }
}
