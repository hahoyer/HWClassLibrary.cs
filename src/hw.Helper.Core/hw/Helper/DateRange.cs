using System.Globalization;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Half-open <c>[Start, End)</c> interval over <see cref="DateTime" />. Includes a few
///     calendar-aware helpers for week- and month-aligned slicing.
/// </summary>
[PublicAPI]
public sealed class DateRange
{
    /// <summary>Exclusive upper bound of the range.</summary>
    public DateTime End;

    /// <summary>Inclusive lower bound of the range.</summary>
    public DateTime Start;

    /// <summary>Duration of the range. Setting this moves <see cref="End" /> while keeping <see cref="Start" /> fixed.</summary>
    public TimeSpan Length
    {
        get => End - Start;
        set => End = Start + value;
    }

    /// <summary>
    ///     Calendar weeks fully covered by this range (every day of the week lies in
    ///     <c>[Start, End)</c>). The week number follows
    ///     <see cref="CultureInfo.CurrentCulture" /> rules.
    /// </summary>
    public IEnumerable<DateRange> SelectContainingWeeks
        => ((int)(End - Start).TotalDays)
            .Select(i => Start + TimeSpan.FromDays(i))
            .GroupBy(d => d.Year * 52 + d.WeekNumber(CultureInfo.CurrentCulture))
            .Select(g => new DateRange { Start = g.Min(), End = (g.Max() + TimeSpan.FromDays(1)).Date })
            .Where(w => (w.End - w.Start).TotalDays >= 7);

    /// <summary>Calendar weeks that overlap with this range, even partially.</summary>
    public IEnumerable<DateRange> SelectContainedWeeks
        => new DateRange
            {
                Start = Start - TimeSpan.FromDays(6), End = End + TimeSpan.FromDays(6)
            }
            .SelectContainingWeeks;

    /// <summary>The calendar week preceding today.</summary>
    public static DateRange LastWeek
        => new DateRange
            {
                Start = DateTime.Today - TimeSpan.FromDays(7), Length = TimeSpan.FromDays(1)
            }
            .SelectContainedWeeks
            .Single();

    /// <summary>The calendar month preceding the current one.</summary>
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

    /// <summary>Calendar months fully covered by this range.</summary>
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

    /// <summary>Days inside this range that fall on the first of a month — i.e. month boundaries.</summary>
    public IEnumerable<DateTime> SplitByMonth => Split(TimeSpan.FromDays(1)).Where(day => day.Day == 1);

    /// <summary>
    ///     Compact display of the range: same day → date, same month → year+day-range, otherwise
    ///     full <c>yyyy.MM.dd…yyyy.MM.dd</c>. Returns <c>null</c> when either bound has a non-zero
    ///     time component (i.e. is not date-aligned).
    /// </summary>
    public string? Format
    {
        get
        {
            if(Start.TimeOfDay != TimeSpan.FromHours(0) || End.TimeOfDay != TimeSpan.FromHours(0))
                return null;
            if(Start.Year != End.Year)
                return $"{Start:yyyy.MM.dd}...{End:yyyy.MM.dd}";
            if(Start.Month != End.Month)
                return $"{Start:yyyy.MM.dd}...{End:MM.dd}";
            return $"{Start:yyyy.MM.dd}...{End:dd}";
        }
    }

    /// <summary>Yields the lower bound and every subsequent <paramref name="interval" />-step until <see cref="End" />.</summary>
    public IEnumerable<DateTime> Split(TimeSpan interval)
    {
        for(var result = Start; result < End; result += interval)
            yield return result;
    }

    /// <summary>Whether <paramref name="target" /> lies in <c>[Start, End)</c>.</summary>
    public bool Contains(DateTime target) => Start <= target && target < End;
}