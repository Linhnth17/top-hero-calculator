using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class GearStore
{
    public IReadOnlyList<GearTable> All { get; }

    public GearStore(IWebHostEnvironment env, ResourceCatalog catalog)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "gear.json");
        All = JsonSerializer.Deserialize<List<GearTable>>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        foreach (var t in All)
            t.Resources = t.ResourceIds
                .Select(id => catalog.Get(id)
                    ?? throw new InvalidOperationException($"Nguyên liệu '{id}' chưa có trong Data/resources.json"))
                .ToList();
    }

    public GearTable? Get(string? id) => All.FirstOrDefault(t => t.Id == id);

    /// Nối nhiều bảng thành một lộ trình: hết bảng trước mới sang bảng sau
    /// (ví dụ nâng cấp 0→40 rồi mới đột phá sao). Mức trong lộ trình = tổng mức các bảng.
    public GearTable Chain(string name, params string[] ids)
    {
        var tables = ids.Select(id => Get(id) ?? throw new InvalidOperationException($"Chưa có bảng '{id}' trong Data/gear.json")).ToList();
        var resIds = tables.SelectMany(t => t.ResourceIds).Distinct().ToList();
        var rows = new List<GearRow>();
        foreach (var t in tables)
            foreach (var r in t.Rows.OrderBy(r => r.Level))
            {
                var costs = new long[resIds.Count];
                for (int i = 0; i < t.ResourceIds.Count; i++) costs[resIds.IndexOf(t.ResourceIds[i])] = r.Costs[i];
                rows.Add(new GearRow { Level = rows.Count + 1, Costs = costs });
            }

        return new GearTable
        {
            Id = string.Join("+", ids),
            Name = name,
            ResourceIds = resIds,
            Resources = resIds.Select(id => tables.SelectMany(t => t.Resources).First(r => r.Id == id)).ToList(),
            CostMeaning = "reach",
            Rows = rows
        };
    }
}
