namespace TicketingSystem.Helpers;

public static class TimeFormatHelper
{
    public static string FormatMinutes(int minutes)
    {
        if (minutes <= 0)
        {
            return "0m";
        }

        var hours = minutes / 60;
        var remainder = minutes % 60;

        if (hours > 0 && remainder > 0)
        {
            return $"{hours}h {remainder}m";
        }

        if (hours > 0)
        {
            return $"{hours}h";
        }

        return $"{remainder}m";
    }
}
