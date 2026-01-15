using TicketingSystem.Models;
using TicketingSystem.Options;

namespace TicketingSystem.Helpers;

public enum SlaState
{
    OnTrack = 0,
    DueSoon = 1,
    Overdue = 2
}

public static class SlaHelper
{
    public static double GetAgeHours(DateTime createdAtUtc, DateTime nowUtc)
    {
        var span = nowUtc - createdAtUtc;
        if (span < TimeSpan.Zero)
        {
            return 0;
        }

        return span.TotalHours;
    }

    public static string FormatAge(DateTime createdAtUtc, DateTime nowUtc)
    {
        var span = nowUtc - createdAtUtc;
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }

        if (span.TotalDays >= 1)
        {
            return $"{(int)span.TotalDays}d {span.Hours}h";
        }

        if (span.TotalHours >= 1)
        {
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        }

        var minutes = Math.Max(1, span.Minutes);
        return $"{minutes}m";
    }

    public static SlaState GetSlaState(DateTime createdAtUtc, TicketPriority priority, SlaOptions options)
    {
        var threshold = options.GetThresholdHours(priority);
        if (threshold <= 0)
        {
            return SlaState.OnTrack;
        }

        var ageHours = GetAgeHours(createdAtUtc, DateTime.UtcNow);
        if (ageHours >= threshold)
        {
            return SlaState.Overdue;
        }

        if (ageHours >= threshold * 0.75)
        {
            return SlaState.DueSoon;
        }

        return SlaState.OnTrack;
    }
}
