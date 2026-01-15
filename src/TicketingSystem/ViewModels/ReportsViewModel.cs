namespace TicketingSystem.ViewModels;

public class ReportsViewModel
{
    public DateTime GeneratedAtUtc { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public int TotalOpenCount { get; set; }
    public int ClosedLast30DaysCount { get; set; }
    public int OnTrackCount { get; set; }
    public int DueSoonCount { get; set; }
    public int OverdueCount { get; set; }
    public List<string> VolumeLabels { get; set; } = new();
    public List<int> VolumeCreatedCounts { get; set; } = new();
    public List<int> VolumeClosedCounts { get; set; } = new();
    public List<ReportsWorkloadItem> WorkloadItems { get; set; } = new();
}

public class ReportsWorkloadItem
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
