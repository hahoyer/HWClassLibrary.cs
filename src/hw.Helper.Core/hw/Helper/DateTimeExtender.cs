using System.Globalization;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>Extension methods around <see cref="DateTime" /> and <see cref="TimeSpan" />.</summary>
[PublicAPI]
public static class DateTimeExtender
{
    extension(DateTime dateTime)
    {
        /// <summary>
        ///     Formats <paramref name="dateTime" /> as <c>HH:mm:ss.fff dd.MM.yy</c> using fixed-width,
        ///     culture-independent fields.
        /// </summary>
        public string Format()
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            result += ".";
            result += dateTime.Millisecond.ToString("000");
            result += " ";
            result += dateTime.Day.ToString("00");
            result += ".";
            result += dateTime.Month.ToString("00");
            result += ".";
            result += dateTime.Year.ToString("00");
            return result;
        }

        /// <summary>
        ///     Like <see cref="Format" />, but omits date components that match today's date so that
        ///     same-day timestamps render as time-only and same-month timestamps drop the year.
        /// </summary>
        /// <param name="showMilliseconds"> Whether to include the <c>.fff</c> milliseconds suffix. </param>
        public string DynamicShortFormat(bool showMilliseconds)
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            if (showMilliseconds)
            {
                result += ".";
                result += dateTime.Millisecond.ToString("000");
            }

            var nowDate = DateTime.Now.Date;
            var sameYear = nowDate.Year == dateTime.Year;
            var sameMonth = sameYear && nowDate.Month == dateTime.Month;
            var sameDay = sameMonth && nowDate.Day == dateTime.Day;

            if (!sameDay)
            {
                result += " ";
                result += dateTime.Day.ToString("00");
                result += ".";
            }

            if (!sameMonth)
            {
                result += dateTime.Month.ToString("00");
                result += ".";
            }

            if (!sameYear)
                result += dateTime.Year.ToString("00");
            return result;
        }

        /// <summary>
        ///     ISO-style week-of-year computation that follows the calendar rules of
        ///     <paramref name="culture" /> (default: <see cref="CultureInfo.CurrentCulture" />).
        /// </summary>
        public int WeekNumber(CultureInfo? culture)
        {
            culture ??= CultureInfo.CurrentCulture;
            var dateTimeFormatInfo = culture.DateTimeFormat;
            return dateTimeFormatInfo
                .Calendar
                .GetWeekOfYear(dateTime, dateTimeFormatInfo.CalendarWeekRule, dateTimeFormatInfo.FirstDayOfWeek);
        }
    }

    /// <summary>
    ///     Formats <paramref name="timeSpan" /> with at most three significant digits and an
    ///     appropriate unit (days, hours, minutes, seconds down to nanoseconds). Optionally renders
    ///     minute/second markers with the typographic <c>'</c>/<c>"</c> symbols.
    /// </summary>
    /// <param name="timeSpan"> The duration to format. </param>
    /// <param name="omitZeros"> If <c>true</c>, hides zero subordinate fields. </param>
    /// <param name="useSymbols"> If <c>true</c>, uses <c>'</c>/<c>"</c> for minutes/seconds; otherwise <c>m</c>/<c>s</c>. </param>
    public static string Format3Digits(this TimeSpan timeSpan, bool omitZeros = true, bool useSymbols = true)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{timeSpan.TotalDays:0.00}d";
        if (timeSpan.Hours > 0)
            return $"{timeSpan.Hours}{OmitCheck(":", timeSpan.Minutes, omitZeros)}h";
        if (timeSpan.Minutes > 0)
            return $"{timeSpan.Minutes}{OmitCheck(":", timeSpan.Seconds, omitZeros)}{(useSymbols ? "'" : "m")}";

        var nanoSeconds = ((long)(timeSpan.TotalMilliseconds * 1000 * 1000)).Format3Digits(omitZeros) + "ns";
        return nanoSeconds.Replace("kns", "�s").Replace("Mns", "ms").Replace("Gns", useSymbols ? "\"" : "s");
    }

    /// <summary>Reads <paramref name="value" /> as a number of seconds, e.g. <c>1.5.Seconds()</c>.</summary>
    public static TimeSpan Seconds(this double value) => TimeSpan.FromSeconds(value);

    /// <inheritdoc cref="Seconds(double)" />
    public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);

    /// <summary>Reads <paramref name="value" /> as a number of minutes.</summary>
    public static TimeSpan Minutes(this double value) => TimeSpan.FromMinutes(value);

    /// <inheritdoc cref="Minutes(double)" />
    public static TimeSpan Minutes(this int value) => TimeSpan.FromMinutes(value);

    /// <summary>Reads <paramref name="value" /> as a number of hours.</summary>
    public static TimeSpan Hours(this double value) => TimeSpan.FromHours(value);

    /// <inheritdoc cref="Hours(double)" />
    public static TimeSpan Hours(this int value) => TimeSpan.FromHours(value);

    /// <summary>Reads <paramref name="value" /> as a number of days.</summary>
    public static TimeSpan Days(this double value) => TimeSpan.FromDays(value);

    /// <inheritdoc cref="Days(double)" />
    public static TimeSpan Days(this int value) => TimeSpan.FromDays(value);

    /// <summary>
    ///     Sleeps for the given duration; non-positive durations are silently ignored, so this can
    ///     be called with computed deadlines without prior bounds-checking.
    /// </summary>
    public static void Sleep(this TimeSpan value)
    {
        if (value.Ticks > 0)
            Thread.Sleep(value);
    }

    static string OmitCheck(string delimiter, int value, bool omitZeros)
    {
        if (omitZeros && value == 0)
            return "";
        return delimiter + value.ToString("00");
    }
}