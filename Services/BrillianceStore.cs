using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class BrillianceStore
{
    public GearTable Table { get; }

    public BrillianceStore(IWebHostEnvironment env, ResourceCatalog catalog)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "brilliance.json");
        Table = JsonSerializer.Deserialize<GearTable>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        Table.Resources = Table.ResourceIds
            .Select(id => catalog.Get(id)
                ?? throw new InvalidOperationException($"Nguyên liệu '{id}' chưa có trong Data/resources.json"))
            .ToList();
    }
}
