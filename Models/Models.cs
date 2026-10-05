namespace SpendCalc.Models;

public class EventDef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ItemInput> Items { get; set; } = new();
}

public class ItemInput
{
    public string Name { get; set; } = "";
    public long Cost { get; set; }        // chi phí cho 1 lần
    public int Points { get; set; }       // điểm cho 1 lần
    public int? MaxTimes { get; set; }    // null = không giới hạn
}

public record PlanLine(string Name, int Times, long Cost, long Points);

public class PlanResult
{
    public bool Reachable { get; init; }
    public long MinCost { get; init; }
    public long TotalPoints { get; init; }
    public List<PlanLine> Lines { get; init; } = new();
}

public class PlannerVm
{
    public string EventId { get; set; } = "";
    public int Target { get; set; }
    public List<ItemInput> Items { get; set; } = new();
    public List<EventDef> Events { get; set; } = new();
    public PlanResult? Result { get; set; }
    public string? Error { get; set; }
}
