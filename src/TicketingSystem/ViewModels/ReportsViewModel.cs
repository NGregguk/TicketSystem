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
    public int TimeLoggedMinutes30 { get; set; }
    public List<ReportsTimeTicketItem> TopTimeTickets { get; set; } = new();
    public List<string> VolumeLabels { get; set; } = new();
    public List<int> VolumeCreatedCounts { get; set; } = new();
    public List<int> VolumeClosedCounts { get; set; } = new();
    public List<string> VolumeLabels14 { get; set; } = new();
    public List<int> VolumeCreatedCounts14 { get; set; } = new();
    public List<int> VolumeClosedCounts14 { get; set; } = new();
    public List<string> VolumeDateKeys { get; set; } = new();
    public List<string> VolumeDateKeys14 { get; set; } = new();
    public List<ReportsWorkloadItem> WorkloadItems { get; set; } = new();
    public List<ReportsTicketItem> UnassignedTickets { get; set; } = new();
}

public class ReportsWorkloadItem
{
    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportsTicketItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? Category { get; set; }
}

public class ReportsTimeTicketItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Minutes { get; set; }
}
