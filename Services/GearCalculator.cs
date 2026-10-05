using SpendCalc.Models;

namespace SpendCalc.Services;

public static class GearCalculator
{
    public static GearResult Calculate(GearTable table, int from, int to)
    {
        if (from >= to)
            throw new ArgumentException("Level muốn đạt phải lớn hơn level hiện tại.");

        bool reach = table.CostMeaning == "reach";
        int lo = reach ? from + 1 : from;
        int hi = reach ? to : to - 1;

        var steps = new List<GearRow>();
        for (int level = lo; level <= hi; level++)
        {
            var row = table.Rows.FirstOrDefault(r => r.Level == level)
                ?? throw new ArgumentException($"Chưa có dữ liệu cho level {level}.");
            steps.Add(row);
        }

        var totals = new long[table.Resources.Count];
        foreach (var s in steps)
            for (int i = 0; i < totals.Length; i++) totals[i] += s.Costs[i];

        return new GearResult { Totals = totals, Steps = steps };
    }

    /// Với số nguyên liệu đang có, nâng cùng lúc `pieces` món từ level `from`, đạt được level nào.
    public static GearReach MaxReach(GearTable table, int from, IReadOnlyList<long?> own, int pieces = 1)
    {
        int n = table.Resources.Count;
        var remaining = new long[n];
        for (int i = 0; i < n; i++) remaining[i] = i < own.Count ? (own[i] ?? 0) : 0;
        var spent = new long[n];
        bool reach = table.CostMeaning == "reach";
        int level = from;
        int? limit = null;
        bool outOfData = false;

        while (true)
        {
            int rowLevel = reach ? level + 1 : level;
            var row = table.Rows.FirstOrDefault(r => r.Level == rowLevel);
            if (row == null) { outOfData = true; break; }

            int lack = -1;
            for (int i = 0; i < n; i++)
                if (remaining[i] < row.Costs[i] * pieces) { lack = i; break; }
            if (lack >= 0) { limit = lack; break; }

            for (int i = 0; i < n; i++)
            {
                long c = row.Costs[i] * pieces;
                remaining[i] -= c;
                spent[i] += c;
            }
            level++;
        }
        return new GearReach { Level = level, Spent = spent, Remaining = remaining, LimitingIndex = limit, OutOfData = outOfData };
    }
}
