using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace hw.Helper
{
    sealed class DateRange
    {
        public DateTime Start;
        public DateTime End;

        internal IEnumerable<DateRange> SelectContainingWeeks
        {
            get
            {
                return ((int)(End - Start).TotalDays)
                    .Select(i => Start + TimeSpan.FromDays(i))
                    .GroupBy(d => d.Year * 52 + d.WeekNumber(CultureInfo.CurrentCulture))
                    .Select(g => new DateRange { Start = g.Min(), End = (g.Max() + TimeSpan.FromDays(1)).Date })
                    .Where(w => (w.End - w.Start).TotalDays >= 7);
            }
        }

        internal IEnumerable<DateTime> Split(TimeSpan interval)
        {
            for (var result = Start; result < End; result += interval)
                yield return result;
        }

        internal String Format
        {
            get
            {
                if (Start.TimeOfDay != TimeSpan.FromHours(0) || End.TimeOfDay != TimeSpan.FromHours(0))
                    return null;
                if (Start.Year != End.Year)
                    return Start.ToString("yyyy.MM.dd") + "..." + End.ToString("yyyy.MM.dd");
                if (Start.Month != End.Month)
                    return Start.Year + " " + Start.ToString("MM.dd") + "..." + End.ToString("MM.dd");
                return Start.ToString("yyyy.MM") + " " + Start.Day + "..." + End.Day;
            }
        }
    }
}