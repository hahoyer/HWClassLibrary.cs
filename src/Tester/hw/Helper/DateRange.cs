using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public sealed class DateRange
{
    public DateTime End;
    public DateTime Start;

    public TimeSpan Length
    {
        get => End - Start;
        set => End = Start + value;
    }

    public IEnumerable<DateRange> SelectContainingWeeks
        => ((int)(End - Start).TotalDays)
            .Select(i => Start + TimeSpan.FromDays(i))
            .GroupBy(d => d.Year * 52 + d.WeekNumber(CultureInfo.CurrentCulture))
            .Select(g => new DateRange { Start = g.Min(), End = (g.Max() + TimeSpan.FromDays(1)).Date })
            .Where(w => (w.End - w.Start).TotalDays >= 7);

    public IEnumerable<DateRange> SelectContainedWeeks
        => new DateRange
            {
                Start = Start - TimeSpan.FromDays(6), End = End + TimeSpan.FromDays(6)
            }
            .SelectContainingWeeks;

    public static DateRange LastWeek
        => new DateRange
            {
                Start = DateTime.Today - TimeSpan.FromDays(7), Length = TimeSpan.FromDays(1)
            }
            .SelectContainedWeeks
            .Single();

    public static DateRange LastMonth
    {
        get
        {
            var today = DateTime.Today;
            var result = new DateRange { End = new(today.Year, today.Month, 1) };
            var endOfLastMonth = result.End - TimeSpan.FromDays(1);
            result.Start = new(endOfLastMonth.Year, endOfLastMonth.Month, 1);
            return result;
        }
    }

    public IEnumerable<DateRange> SelectContainingMonths
    {
        get
        {
            var monthStarts = SplitByMonth.ToArray();
            foreach(var start in monthStarts)
            {
                var end = start.AddMonths(1);
                if(end <= End)
                    yield return new() { Start = start, End = end };
            }
        }
    }

    public IEnumerable<DateTime> SplitByMonth => Split(TimeSpan.FromDays(1)).Where(day => day.Day == 1);

    public string Format
    {
        get
        {
            if(Start.TimeOfDay != TimeSpan.FromHours(0) || End.TimeOfDay != TimeSpan.FromHours(0))
                return null;
            if(Start.Year != End.Year)
                return Start.ToString("yyyy.MM.dd") + "..." + End.ToString("yyyy.MM.dd");
            if(Start.Month != End.Month)
                return Start.Year + " " + Start.ToString("MM.dd") + "..." + End.ToString("MM.dd");
            return Start.ToString("yyyy.MM") + " " + Start.Day + "..." + End.Day;
        }
    }

    public IEnumerable<DateTime> Split(TimeSpan interval)
    {
        for(var result = Start; result < End; result += interval)
            yield return result;
    }

    public bool Contains(DateTime target) => Start <= target && target < End;
}