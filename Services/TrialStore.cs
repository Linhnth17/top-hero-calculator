using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class TrialStore
{
    public IReadOnlyList<TrialEvent> All { get; }

    public TrialStore(IWebHostEnvironment env, ResourceCatalog catalog)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "trial-events.json");
        All = JsonSerializer.Deserialize<List<TrialEvent>>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        foreach (var e in All)
        {
            foreach (var s in e.Sources)
                s.Resource = catalog.Get(s.ResourceId)
                    ?? throw new InvalidOperationException($"Nguyên liệu '{s.ResourceId}' chưa có trong Data/resources.json");
            e.Milestones = e.Milestones.OrderBy(m => m.Points).ToList();
        }
    }

    /// Tính điểm sự kiện khi dùng hết số nguyên liệu `amountOf(resourceId)`.
    public static TrialResult Score(TrialEvent ev, Func<string, long> amountOf)
    {
        var amounts = ev.Sources.Select(s => amountOf(s.ResourceId)).ToArray();
        var points = ev.Sources.Select((s, i) => s.Score(amounts[i])).ToArray();
        long total = points.Sum();

        return new TrialResult
        {
            Amounts = amounts,
            SourcePoints = points,
            Total = total,
            ReachedCount = ev.Milestones.Count(m => m.Points <= total),
            Next = ev.Milestones.FirstOrDefault(m => m.Points > total)
        };
    }
}
