namespace TicketingSystem.Services;

public sealed class BusinessTimeCalculator
{
    public static BusinessTimeCalculator Default { get; } = new(WorkSchedule.CreateDefault());

    private readonly WorkSchedule _schedule;

    public BusinessTimeCalculator(WorkSchedule schedule)
    {
        _schedule = schedule;
    }

    public int GetWorkingMinutesElapsed(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
        {
            return 0;
        }

        var startLocal = ToScheduleLocal(startUtc);
        var endLocal = ToScheduleLocal(endUtc);
        if (endLocal <= startLocal)
        {
            return 0;
        }

        var totalMinutes = 0;
        var cursorDate = startLocal.Date;
        var endDate = endLocal.Date;

        while (cursorDate <= endDate)
        {
            if (_schedule.WorkDays.Contains(cursorDate.DayOfWeek))
            {
                var workStart = cursorDate.Add(_schedule.StartTime);
                var workEnd = cursorDate.Add(_schedule.EndTime);

                var rangeStart = cursorDate == startLocal.Date ? startLocal : workStart;
                var rangeEnd = cursorDate == endLocal.Date ? endLocal : workEnd;

                var overlapStart = rangeStart > workStart ? rangeStart : workStart;
                var overlapEnd = rangeEnd < workEnd ? rangeEnd : workEnd;

                if (overlapEnd > overlapStart)
                {
                    totalMinutes += (int)Math.Floor((overlapEnd - overlapStart).TotalMinutes);
                }
            }

            cursorDate = cursorDate.AddDays(1);
        }

        return totalMinutes;
    }

    public DateTime AddWorkingMinutes(DateTime startUtc, int minutesToAdd)
    {
        if (minutesToAdd <= 0)
        {
            return startUtc;
        }

        var current = ToScheduleLocal(startUtc);

        while (minutesToAdd > 0)
        {
            if (!_schedule.WorkDays.Contains(current.DayOfWeek))
            {
                current = NextWorkDayStart(current.Date);
                continue;
            }

            var workStart = current.Date.Add(_schedule.StartTime);
            var workEnd = current.Date.Add(_schedule.EndTime);

            if (current < workStart)
            {
                current = workStart;
            }
            else if (current >= workEnd)
            {
                current = NextWorkDayStart(current.Date.AddDays(1));
                continue;
            }

            var availableMinutes = (int)Math.Floor((workEnd - current).TotalMinutes);
            if (availableMinutes <= 0)
            {
                current = NextWorkDayStart(current.Date.AddDays(1));
                continue;
            }

            var minutesToConsume = Math.Min(availableMinutes, minutesToAdd);
            current = current.AddMinutes(minutesToConsume);
            minutesToAdd -= minutesToConsume;

            if (minutesToAdd > 0)
            {
                current = NextWorkDayStart(current.Date.AddDays(1));
            }
        }

        return FromScheduleLocal(current);
    }

    private DateTime ToScheduleLocal(DateTime utcValue)
    {
        var utc = utcValue.Kind == DateTimeKind.Utc
            ? utcValue
            : DateTime.SpecifyKind(utcValue, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, _schedule.TimeZone);
    }

    private DateTime FromScheduleLocal(DateTime localValue)
    {
        var unspecifiedLocal = DateTime.SpecifyKind(localValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedLocal, _schedule.TimeZone);
    }

    private DateTime NextWorkDayStart(DateTime date)
    {
        var cursor = date;
        while (!_schedule.WorkDays.Contains(cursor.DayOfWeek))
        {
            cursor = cursor.AddDays(1);
        }

        return cursor.Add(_schedule.StartTime);
    }
}
