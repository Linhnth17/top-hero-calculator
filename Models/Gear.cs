using System.Text.Json.Serialization;

namespace SpendCalc.Models;

public class ResourceDef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Icon { get; set; }   // ví dụ "/images/resources/res1.png"; null/trống = không hiện ảnh
}

public class GearRow
{
    public int Level { get; set; }
    public long[] Costs { get; set; } = Array.Empty<long>();   // theo thứ tự Resources
    public string? Requires { get; set; }    // công trình cần có (Brilliance)
    public string? Milestone { get; set; }   // mốc đạt được ở level này (Brilliance)
}

public class GearTable
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public List<string> ResourceIds { get; set; } = new();                  // đọc từ gear.json
    [JsonIgnore] public List<ResourceDef> Resources { get; set; } = new();  // nạp từ danh mục nguyên liệu
    /// "upgradeFrom": chi phí ở dòng L là để nâng TỪ level L lên L+1.
    /// "reach": chi phí ở dòng L là để ĐẠT level L.
    public string CostMeaning { get; set; } = "upgradeFrom";
    public string LevelLabel { get; set; } = "level";   // "level", "cánh sao"...
    public bool ShowStars { get; set; }                  // hiển thị ngôi sao theo số cánh
    public List<GearRow> Rows { get; set; } = new();
}

public class GearResult
{
    public long[] Totals { get; init; } = Array.Empty<long>();
    public List<GearRow> Steps { get; init; } = new();
}

public class GearReach
{
    public int Level { get; init; }                     // level cao nhất đạt được
    public long[] Spent { get; init; } = Array.Empty<long>();
    public long[] Remaining { get; init; } = Array.Empty<long>();
    public int? LimitingIndex { get; init; }            // nguyên liệu thiếu đầu tiên ở bước kế tiếp
    public bool OutOfData { get; init; }                // dừng vì hết dữ liệu level
}

public class GearVm
{
    public List<GearTable> Tables { get; set; } = new();
    public GearTable Table { get; set; } = new();
    public int From { get; set; }
    public int MaxLevel { get; set; }
    public int Pieces { get; set; } = 4;
    public long[] Totals { get; set; } = Array.Empty<long>();   // tổng chi phí từ 0 đến MaxLevel (đã nhân số món)
    public List<long?> Own { get; set; } = new();
    public GearReach? Reach { get; set; }
    public string? Error { get; set; }
}

public class BrillianceVm
{
    public GearTable Table { get; set; } = new();
    public int MinLevel { get; set; }                    // level castle trước bậc B1 (40)
    public int MaxLevel { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public GearResult? Cost { get; set; }                // chi phí From → To
    public int Cur { get; set; }                         // cấp thành hiện tại cho phần ước tính
    public List<long?> Own { get; set; } = new();
    public GearReach? Reach { get; set; }
    public int? NextTierLevel { get; set; }              // cấp thành của bậc Quang Huy kế tiếp
    public long[]? NextTierLack { get; set; }            // còn thiếu để lên bậc đó

    /// Bậc Brilliance đã hoàn thành ở level castle này (0 = chưa có).
    public static int TierAt(int level) => Math.Max(0, (level - 40) / 5);
    /// Bậc mà một dòng level thuộc về (41-45 → 1, 46-50 → 2...).
    public static int TierOf(int level) => (level - 41) / 5 + 1;
}
