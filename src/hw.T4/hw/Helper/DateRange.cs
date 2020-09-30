using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace hw.Helper
{
    public sealed class DateRange
    {
        public DateTime Start;
        public DateTime End;
        public TimeSpan Length { get { return End - Start; } set { End = Start + value; } }

        public IEnumerable<DateRange> SelectContainingWeeks
        {
            get
            {
                return ((int) (End - Start).TotalDays)
                    .Select(i => Start + TimeSpan.FromDays(i))
                    .GroupBy(d => d.Year * 52 + d.WeekNumber(CultureInfo.CurrentCulture))
                    .Select(g => new DateRange {Start = g.Min(), End = (g.Max() + TimeSpan.FromDays(1)).Date})
                    .Where(w => (w.End - w.Start).TotalDays >= 7);
            }
        }

        public IEnumerable<DateRange> SelectContainedWeeks
        {
            get
            {
                return new DateRange()
                {
                    Start = Start - TimeSpan.FromDays(6),
                    End = End + TimeSpan.FromDays(6)
                }
                    .SelectContainingWeeks;
            }
        }

        public static DateRange LastWeek
        {
            get
            {
                return new DateRange()
                {
                    Start = DateTime.Today - TimeSpan.FromDays(7),
                    Length = TimeSpan.FromDays(1)
                }
                    .SelectContainedWeeks
                    .Single();
            }
        }

        public static DateRange LastMonth
        {
            get
            {
                var today = DateTime.Today;
                var result = new DateRange();
                result.End = new DateTime(today.Year, today.Month, 1);
                var endOfLastMonth = result.End - TimeSpan.FromDays(1);
                result.Start = new DateTime(endOfLastMonth.Year, endOfLastMonth.Month, 1);
                return result;
            }
        }

        public IEnumerable<DateRange> SelectContainingMonths
        {
            get
            {
                var m = SplitByMonth.ToArray();
                for (var i = 0; i < m.Length; i++)
                {
                    var start = m[i];
                    var end = start.AddMonths(1);
                    if (end <= End)
                        yield return new DateRange { Start = start, End = end };
                }
            }
        }
        
        public IEnumerable<DateTime> SplitByMonth { get { return Split(TimeSpan.FromDays(1)).Where(d => d.Day == 1); } }

        public IEnumerable<DateTime> Split(TimeSpan interval)
        {
            for(var result = Start; result < End; result += interval)
                yield return result;
        }

        public String Format
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

        public bool Contains(DateTime target) { return Start <= target && target < End; }
    }
}