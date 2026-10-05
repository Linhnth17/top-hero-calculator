using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class EventStore
{
    public IReadOnlyList<EventDef> All { get; }

    public EventStore(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "events.json");
        All = JsonSerializer.Deserialize<List<EventDef>>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }

    public EventDef? Get(string? id) => All.FirstOrDefault(e => e.Id == id);
}
