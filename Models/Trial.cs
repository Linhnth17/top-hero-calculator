using System.Text.Json.Serialization;

namespace SpendCalc.Models;

public class TrialEvent
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public long LeaderboardMin { get; set; }             // điểm tối thiểu để lên BXH
    public List<PointSource> Sources { get; set; } = new();
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public long Points { get; set; }
}

/// Dùng `Per` món nguyên liệu được `Points` điểm (ví dụ 5 Rune = 1 điểm).
public class PointSource
{
    public string ResourceId { get; set; } = "";
    public string? Label { get; set; }                   // tên trong sự kiện, nếu khác tên nguyên liệu
    public string? Unit { get; set; }                    // tên ngắn để ghi tỉ lệ, ví dụ "Rune"
    public long Per { get; set; } = 1;
    public long Points { get; set; }
    [JsonIgnore] public ResourceDef Resource { get; set; } = new();

    public string Name => Label is not { Length: > 0 } l ? Resource.Name
        : l.Contains(Resource.Name, StringComparison.OrdinalIgnoreCase) ? l
        : $"{Resource.Name} ({l})";
    /// Tỉ lệ quy đổi dễ đọc, ví dụ "5 Rune = 1 điểm".
    public string Rate
    {
        get
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            return $"{Per.ToString("N0", inv)} {Unit ?? Resource.Name} = {Points.ToString("N0", inv)} điểm";
        }
    }

    public long Score(long amount) => amount / Per * Points;
}

public class TrialResult
{
    public long[] Amounts { get; init; } = Array.Empty<long>();   // theo thứ tự Sources
    public long[] SourcePoints { get; init; } = Array.Empty<long>();
    public long Total { get; init; }
    public int ReachedCount { get; init; }                         // số rương mốc đã đạt
    public Milestone? Next { get; init; }                          // rương kế tiếp, null = đã đạt hết
}
