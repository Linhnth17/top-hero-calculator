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
}
