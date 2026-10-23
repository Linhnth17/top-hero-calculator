using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class HeroStore
{
    public HeroData Data { get; }

    public HeroStore(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "heroes.json");
        Data = JsonSerializer.Deserialize<HeroData>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        var setIds = Data.Sets.Select(s => s.Id).ToHashSet();
        foreach (var h in Data.Heroes)
            foreach (var id in h.Alt.Prepend(h.Set).OfType<string>())
                if (!setIds.Contains(id))
                    throw new InvalidOperationException($"Tướng '{h.En}' dùng bộ '{id}' chưa có trong Data/heroes.json");
    }

    public GearSet? Set(string? id) => Data.Sets.FirstOrDefault(s => s.Id == id);
    public string FactionName(string id) => Data.Factions.FirstOrDefault(f => f.Id == id)?.Name ?? id;
}
