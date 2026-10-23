namespace SpendCalc.Models;

public class HeroData
{
    public string Source { get; set; } = "";
    public List<Faction> Factions { get; set; } = new();
    public List<GearSet> Sets { get; set; } = new();
    public List<Hero> Heroes { get; set; } = new();
}

public class Faction
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

/// Bộ trang bị; icon từng món lấy theo SquadVm.Icon(slot, IconSet).
public class GearSet
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string En { get; set; } = "";
    public int IconSet { get; set; }
    public List<string> Bonuses { get; set; } = new();   // chỉ số khi đủ 4 món cấp 40
    public string Fit { get; set; } = "";                // hợp với kiểu tướng nào
}

public class Hero
{
    public string En { get; set; } = "";
    public string? Vi { get; set; }                       // tên trong bản tiếng Việt, null = chưa có
    public string Faction { get; set; } = "";
    public string Rarity { get; set; } = "";              // "mythic" | "legendary"
    public string? Set { get; set; }                      // bộ nên dùng, null = chưa có gợi ý
    public List<string> Alt { get; set; } = new();        // bộ thay thế

    public string Name => Vi ?? En;
    /// Ảnh ở /images/heroes/{slug}.webp, ví dụ "Rose Princess" → "rose-princess".
    public string Portrait => "/images/heroes/" + System.Text.RegularExpressions.Regex.Replace(En.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-') + ".webp";
}
