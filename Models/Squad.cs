namespace SpendCalc.Models;

/// Kết quả nâng cả đội hình theo một cách chọn bước.
public class SquadPlan
{
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    public string Desc { get; init; } = "";
    public int?[] From { get; init; } = Array.Empty<int?>();   // mức trong lộ trình, null = món bỏ qua
    public int?[] To { get; init; } = Array.Empty<int?>();
    public int Steps { get; init; }
    public long[] Spent { get; init; } = Array.Empty<long>();
    public long[] Remaining { get; init; } = Array.Empty<long>();
    public TrialResult Result { get; init; } = new();
}

public class SquadVm
{
    public const int Heroes = 6, Slots = 4;
    public const int MaxLevel = 40;    // cấp tối đa, đạt cấp này mới đột phá sao
    public const int MaxWings = 25;    // 5 sao × 5 cánh
    public const int GearSets = 3;     // số mẫu icon cho mỗi loại đồ

    /// Thứ tự món trong một tướng; icon ở /images/gear/{Key}{bộ}.png
    public static readonly (string Key, string Name)[] SlotDefs =
    {
        ("weapon", "Vũ Khí"), ("armor", "Áo Giáp"), ("boots", "Giày"), ("helmet", "Mũ")
    };
    public static string Icon(int slot, int set) => $"/images/gear/{SlotDefs[slot].Key}{set}.png";

    // Dữ liệu nhập
    public TrialEvent Event { get; set; } = new();
    public GearTable Table { get; set; } = new();               // lộ trình cấp 0→40 rồi sao 0→25
    public List<int?> Levels { get; set; } = new();             // cấp hiện tại, null = bỏ qua
    public List<int> Wings { get; set; } = new();               // số cánh sao hiện tại
    public List<int?> Targets { get; set; } = new();            // target (mức trong lộ trình), null = không đặt
    public List<long?> Own { get; set; } = new();               // theo Table.Resources
    public List<PointSource> ExtraSources { get; set; } = new(); // nguồn điểm không phải nguyên liệu nâng đồ
    public List<long?> Extra { get; set; } = new();
    public List<int> Sets { get; set; } = new();                // bộ icon đang chọn cho từng tướng (1..GearSets)
    public int Tab { get; set; }                                // tướng đang mở

    // Kết quả
    public long[]? TargetNeed { get; set; }                     // nguyên liệu cần để mọi món chạm target
    public int?[]? TargetReached { get; set; }                  // mức đạt được sau khi ưu tiên target
    public long[]? TargetLack { get; set; }                     // còn thiếu để chạm target (null = đủ)
    public List<SquadPlan> Plans { get; set; } = new();
    public SquadPlan? Best { get; set; }

    /// Đổi (cấp, cánh) sang mức trong lộ trình và ngược lại.
    public static int ToStep(int level, int wings) => level + (level >= MaxLevel ? wings : 0);
    public static (int Level, int Wings) FromStep(int step) => (Math.Min(step, MaxLevel), Math.Max(0, step - MaxLevel));

    public static string StepLabel(int step) => step <= MaxLevel ? $"Lv {step}" : WingsLabel(step - MaxLevel);

    public static string WingsLabel(int n) => n == 0 ? "0 sao"
        : n % 5 == 0 ? $"{n / 5} sao"
        : n < 5 ? $"{n} cánh" : $"{n / 5} sao {n % 5} cánh";
}
