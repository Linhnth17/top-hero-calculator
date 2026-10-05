using SpendCalc.Models;

namespace SpendCalc.Services;

/// <summary>
/// Tìm chi phí nhỏ nhất để đạt TỐI THIỂU `target` điểm.
/// Knapsack dạng "phủ": DP trên tổng điểm chính xác, tách giới hạn số lần thành các gói 1,2,4,...
/// Nghiệm tối ưu luôn có tổng điểm &lt; target + (điểm lớn nhất của 1 lần), nên mảng DP chỉ cần cỡ đó.
/// </summary>
public static class MinCostSolver
{
    public const int MaxStates = 300_000;
    private const long Inf = long.MaxValue / 4;

    private record Bundle(int Item, int Count, int Points, long Cost);

    public static PlanResult Solve(IReadOnlyList<ItemInput> input, int target)
    {
        if (target <= 0) return new PlanResult { Reachable = true };

        var items = input.Where(i => i.Points > 0 && i.Cost >= 0 && (i.MaxTimes ?? 1) > 0).ToList();
        if (items.Count == 0) return new PlanResult { Reachable = false };

        int upper = target + items.Max(i => i.Points) - 1;
        if (upper > MaxStates)
            throw new ArgumentException($"Mục tiêu quá lớn (giới hạn khoảng {MaxStates} điểm). Hãy chia nhỏ đơn vị điểm.");

        // 1) Tách mỗi món thành các gói 1,2,4,... để thành bài 0/1
        var bundles = new List<Bundle>();
        for (int idx = 0; idx < items.Count; idx++)
        {
            var it = items[idx];
            long need = (upper + it.Points - 1) / it.Points;          // không cần nhiều hơn mức này
            long remaining = Math.Min(it.MaxTimes ?? long.MaxValue, need);
            for (long chunk = 1; remaining > 0; chunk *= 2)
            {
                int take = (int)Math.Min(chunk, remaining);
                long pts = (long)take * it.Points;
                if (pts <= upper) bundles.Add(new Bundle(idx, take, (int)pts, take * it.Cost));
                remaining -= take;
            }
        }

        // 2) DP: dp[p] = chi phí nhỏ nhất để có ĐÚNG p điểm
        var dp = new long[upper + 1];
        Array.Fill(dp, Inf);
        dp[0] = 0;
        var took = new bool[bundles.Count][];
        for (int b = 0; b < bundles.Count; b++)
        {
            var bd = bundles[b];
            took[b] = new bool[upper + 1];
            for (int p = upper - bd.Points; p >= 0; p--)
            {
                if (dp[p] >= Inf) continue;
                long c = dp[p] + bd.Cost;
                if (c < dp[p + bd.Points]) { dp[p + bd.Points] = c; took[b][p + bd.Points] = true; }
            }
        }

        // 3) Chọn tổng điểm >= target có chi phí nhỏ nhất
        int best = -1;
        for (int p = target; p <= upper; p++)
            if (dp[p] < Inf && (best < 0 || dp[p] < dp[best])) best = p;
        if (best < 0) return new PlanResult { Reachable = false };

        // 4) Truy vết ngược để biết mỗi món dùng mấy lần
        var times = new int[items.Count];
        int q = best;
        for (int b = bundles.Count - 1; b >= 0; b--)
            if (took[b][q]) { times[bundles[b].Item] += bundles[b].Count; q -= bundles[b].Points; }

        var lines = items.Select((it, i) => new PlanLine(it.Name, times[i], times[i] * it.Cost, (long)times[i] * it.Points))
                         .Where(l => l.Times > 0).ToList();
        return new PlanResult { Reachable = true, MinCost = dp[best], TotalPoints = best, Lines = lines };
    }
}
