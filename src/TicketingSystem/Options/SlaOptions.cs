using TicketingSystem.Models;

namespace TicketingSystem.Options;

public class SlaOptions
{
    public const string SectionName = "Sla";

    public int NoneHours { get; set; } = 72;
    public int LowHours { get; set; } = 48;
    public int MediumHours { get; set; } = 24;
    public int HighHours { get; set; } = 8;
    public int CriticalHours { get; set; } = 4;

    public int GetThresholdHours(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => CriticalHours,
            TicketPriority.High => HighHours,
            TicketPriority.Medium => MediumHours,
            TicketPriority.Low => LowHours,
            TicketPriority.None => NoneHours,
            _ => MediumHours
        };
    }
}
