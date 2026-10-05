using System.Text.Json;
using SpendCalc.Models;

namespace SpendCalc.Services;

public class ResourceCatalog
{
    public IReadOnlyList<ResourceDef> All { get; }

    public ResourceCatalog(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "resources.json");
        All = JsonSerializer.Deserialize<List<ResourceDef>>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        foreach (var r in All)
        {
            if (string.IsNullOrWhiteSpace(r.Icon)) { r.Icon = null; continue; }
            var file = Path.Combine(webRoot, r.Icon.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(file)) r.Icon = null;   // chưa có ảnh thì chỉ hiện tên
        }
    }

    public ResourceDef? Get(string id) => All.FirstOrDefault(r => r.Id == id);
}
