using SpendCalc.Models;

namespace SpendCalc.Services;

/// Gợi ý nâng nhiều món (ví dụ 16 món của 1 đội hình) để được nhiều điểm sự kiện nhất.
/// Mỗi món vẫn nâng lần lượt từng bước theo bảng; chỉ khác nhau ở thứ tự chọn món để nâng.
public static class SquadPlanner
{
    record Strategy(string Key, string Name, string Desc);

    static readonly Strategy[] Strategies =
    {
        new("even", "Nâng đều", "Luôn nâng món đang thấp nhất trước."),
        new("value", "Tối ưu điểm", "Chọn bước cho nhiều điểm nhất so với phần nguyên liệu nó tiêu tốn."),
        new("focus", "Dồn từng món", "Dồn nguyên liệu nâng món cao nhất lên trước.")
    };

    /// Nguyên liệu cần để đưa các món có target lên đúng target (nâng lần lượt từng bước).
    public static long[] TargetCost(GearTable table, IReadOnlyList<int?> levels, IReadOnlyList<int?> targets)
    {
        var need = new long[table.Resources.Count];
        for (int k = 0; k < levels.Count; k++)
        {
            if (levels[k] is not int lv || k >= targets.Count || targets[k] is not int target) continue;
            for (int l = lv; l < target && NextRow(table, l) is { } row; l++)
                for (int i = 0; i < need.Length; i++) need[i] += row.Costs[i];
        }
        return need;
    }

    /// Kết quả dồn nguyên liệu cho các món có target.
    public record TargetPhase(int?[] Levels, long[] Spent, long[] Remaining, int Steps);

    /// Nâng các món có target trước (món thấp nhất trước), được tới đâu hay tới đó.
    public static TargetPhase ApplyTargets(GearTable table, IReadOnlyList<int?> levels,
        IReadOnlyList<int?> targets, IReadOnlyList<long?> own)
    {
        int n = table.Resources.Count;
        var remaining = Enumerable.Range(0, n).Select(i => i < own.Count ? own[i] ?? 0 : 0).ToArray();
        var spent = new long[n];
        var cur = levels.ToArray();
        int steps = 0;
        while (true)
        {
            int pick = -1;
            for (int k = 0; k < cur.Length; k++)
            {
                if (cur[k] is not int lv || k >= targets.Count || targets[k] is not int t || lv >= t) continue;
                if (NextRow(table, lv) is not { } row || Enumerable.Range(0, n).Any(i => remaining[i] < row.Costs[i])) continue;
                if (pick < 0 || lv < cur[pick]!.Value) pick = k;
            }
            if (pick < 0) break;

            var r = NextRow(table, cur[pick]!.Value)!;
            for (int i = 0; i < n; i++) { remaining[i] -= r.Costs[i]; spent[i] += r.Costs[i]; }
            cur[pick]++;
            steps++;
        }
        return new TargetPhase(cur, spent, remaining, steps);
    }

    /// Chạy mọi cách chọn bước với phần nguyên liệu còn lại sau khi đã ưu tiên các target.
    /// Trả về theo thứ tự điểm giảm dần.
    public static List<SquadPlan> Plan(TrialEvent ev, GearTable table, IReadOnlyList<int?> levels,
        TargetPhase phase, Func<string, long> extra)
    {
        return Strategies
            .Select(s => Run(ev, table, levels, phase.Levels, phase.Spent, phase.Remaining, phase.Steps, extra, s))
            .OrderByDescending(p => p.Result.Total)
            .ThenBy(p => Array.FindIndex(Strategies, s => s.Key == p.Key))
            .ToList();
    }

    static SquadPlan Run(TrialEvent ev, GearTable table, IReadOnlyList<int?> levels, int?[] start,
        long[] spent0, long[] remaining0, int steps0, Func<string, long> extra, Strategy s)
    {
        int n = table.Resources.Count;
        var remaining = remaining0.ToArray();
        var spent = spent0.ToArray();
        var cur = start.ToArray();
        // Điểm ước tính cho 1 đơn vị nguyên liệu (dùng để so sánh bước, điểm thật tính lại ở cuối)
        var rate = table.ResourceIds
            .Select(id => ev.Sources.FirstOrDefault(x => x.ResourceId == id) is { } src ? (double)src.Points / src.Per : 0)
            .ToArray();

        int steps = steps0;
        while (true)
        {
            int pick = -1;
            double best = double.NegativeInfinity;
            for (int k = 0; k < cur.Length; k++)
            {
                if (cur[k] is not int lv || NextRow(table, lv) is not { } row) continue;
                if (Enumerable.Range(0, n).Any(i => remaining[i] < row.Costs[i])) continue;

                double score = s.Key switch
                {
                    "even" => -lv,
                    "focus" => lv,
                    _ => Value(row, rate, remaining)
                };
                if (score > best) { best = score; pick = k; }
            }
            if (pick < 0) break;

            var r = NextRow(table, cur[pick]!.Value)!;
            for (int i = 0; i < n; i++) { remaining[i] -= r.Costs[i]; spent[i] += r.Costs[i]; }
            cur[pick]++;
            steps++;
        }

        var result = TrialStore.Score(ev, id => Used(table, id, spent) + extra(id));

        return new SquadPlan
        {
            Key = s.Key, Name = s.Name, Desc = s.Desc,
            From = levels.ToArray(), To = cur, Steps = steps,
            Spent = spent, Remaining = remaining, Result = result
        };
    }

    /// Điểm của bước chia cho tỉ lệ nguyên liệu đang có mà bước đó dùng hết.
    static double Value(GearRow row, double[] rate, long[] remaining)
    {
        double points = 0, used = 0;
        for (int i = 0; i < rate.Length; i++)
        {
            points += row.Costs[i] * rate[i];
            if (row.Costs[i] > 0) used += (double)row.Costs[i] / remaining[i];
        }
        return used > 0 ? points / used : points;
    }

    static GearRow? NextRow(GearTable table, int level)
    {
        int rowLevel = table.CostMeaning == "reach" ? level + 1 : level;
        return table.Rows.FirstOrDefault(r => r.Level == rowLevel);
    }

    static long Used(GearTable table, string id, long[] amounts)
    {
        int i = table.ResourceIds.IndexOf(id);
        return i >= 0 ? amounts[i] : 0;
    }
}
