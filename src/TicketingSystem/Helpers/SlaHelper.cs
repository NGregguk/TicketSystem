using TicketingSystem.Models;
using TicketingSystem.Options;
using TicketingSystem.Services;

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
        var minutes = BusinessTimeCalculator.Default.GetWorkingMinutesElapsed(createdAtUtc, nowUtc);
        return minutes / 60d;
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
        var thresholdHours = options.GetThresholdHours(priority);
        if (thresholdHours <= 0)
        {
            return SlaState.OnTrack;
        }

        var thresholdMinutes = (int)Math.Round(thresholdHours * 60d);
        var elapsedMinutes = BusinessTimeCalculator.Default.GetWorkingMinutesElapsed(createdAtUtc, DateTime.UtcNow);
        if (elapsedMinutes >= thresholdMinutes)
        {
            return SlaState.Overdue;
        }

        if (elapsedMinutes >= thresholdMinutes * 0.75)
        {
            return SlaState.DueSoon;
        }

        return SlaState.OnTrack;
    }
}
