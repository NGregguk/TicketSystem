namespace TicketingSystem.Services;

public sealed record WorkSchedule(
    TimeSpan StartTime,
    TimeSpan EndTime,
    HashSet<DayOfWeek> WorkDays,
    TimeZoneInfo TimeZone)
{
    public static WorkSchedule CreateDefault()
    {
        return new WorkSchedule(
            TimeSpan.FromHours(8.5),
            TimeSpan.FromHours(17.5),
            new HashSet<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            },
            ResolveTimeZone());
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
        catch (TimeZoneNotFoundException)
        {
        }

        return TimeZoneInfo.Utc;
    }
}
