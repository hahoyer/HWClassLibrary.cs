using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[PublicAPI]
public sealed class Profiler
{
    /// <summary>
    ///     Measurement Item
    /// </summary>
    public sealed class Item
    {
        readonly Profiler Profiler;
        ProfileItem ProfileItem;

        internal Item(Profiler profiler, string flag, int stackFrameDepth)
        {
            Profiler = profiler;
            Start(flag, stackFrameDepth + 1);
        }

        /// <summary>
        ///     End of measurement for this item
        /// </summary>
        [PublicAPI]
        public void End()
        {
            (Profiler.Current == ProfileItem).Assert();
            Profiler.AfterAction();
        }

        /// <summary>
        ///     End of measurement for this item and start of new measurement
        /// </summary>
        [PublicAPI]
        public void Next(string flag = "")
        {
            End();
            Start(flag, 1);
        }

        void Start(string flag, int stackFrameDepth)
        {
            Instance.BeforeAction(flag, stackFrameDepth + 1);
            ProfileItem = Profiler.Current;
        }
    }

    sealed class Dumper
    {
        readonly int? Count;
        readonly ProfileItem[] Data;
        readonly TimeSpan SumAll;
        readonly TimeSpan SumMax;
        int Index;
        TimeSpan Sum;

        public Dumper(Profiler profiler, int? count, double hidden)
        {
            Count = count;
            Data = profiler.ProfileItems.Values.OrderByDescending(target => target.Duration).ToArray();
            SumAll = Data.Sum(target => target.Duration);
            SumMax = new((long)(SumAll.Ticks * (1.0 - hidden)));
            Index = 0;
            Sum = new();
        }

        public string Format()
        {
            if(Data.Length == 0)
                return "\n=========== Profile empty ============\n";

            var result = "";
            for(; Index < Data.Length && Index != Count && Sum <= SumMax; Index++)
            {
                var item = Data[Index];
                result += item.Format(Index.ToString());
                Sum += item.Duration;
            }

            var stringAligner = new StringAligner();
            stringAligner.AddFloatingColumn("#");
            stringAligner.AddFloatingColumn("  ");
            stringAligner.AddFloatingColumn("  ");
            stringAligner.AddFloatingColumn("  ");

            result = "\n=========== Profile ==================\n" + stringAligner.Format(result);
            result += "Total:\t" + SumAll.Format3Digits();
            if(Index < Data.Length)
                result +=
                    " (" + (Data.Length - Index) + " not-shown-items " + (SumAll - Sum).Format3Digits() + ")";
            result += "\n======================================\n";
            return result;
        }
    }

    static Profiler Instance = new();

    readonly Dictionary<string, ProfileItem> ProfileItems = new();
    readonly Stack<ProfileItem> Stack = new();
    readonly Stopwatch Stopwatch;
    ProfileItem Current;


    Profiler()
    {
        Current = new("");
        Stopwatch = new();
        Current.Start(Stopwatch.Elapsed);
        Stopwatch.Start();
    }

    /// <summary>
    ///     Provides a standard frame for accumulating and reporting result of measurements, that are contained in the given
    ///     action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="count"> The number of measured expressions in result, default is "null" for "no restriction. </param>
    /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
    public static void Frame(Action action, int? count = null, double hidden = 0.1)
    {
        Reset();
        Instance.InternalMeasure(action, "", 1);
        Format(count, hidden).FlaggedLine();
    }

    /// <summary>
    ///     Provides a standard frame for accumulating and reporting result of measurements, that are contained in the given
    ///     function
    /// </summary>
    /// <param name="function"></param>
    /// <param name="count"> The number of measured expressions in result, default is "null" for "no restriction. </param>
    /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
    public static TResult Frame<TResult>(Func<TResult> function, int? count = null, double hidden = 0.1)
    {
        Reset();
        var result = Instance.InternalMeasure(function, "", 1);
        Format(count, hidden).FlaggedLine();
        return result;
    }

    /// <summary>
    ///     Start measurement of the following code parts until next item.End() statement.
    /// </summary>
    /// <param name="flag"> </param>
    /// <returns> an item, that represents the measurement.</returns>
    public static Item Start(string flag = "") => new(Instance, flag, 1);

    /// <summary>
    ///     Measures the specified expression.
    /// </summary>
    /// <typeparam name="T"> The type the specified expression returns </typeparam>
    /// <param name="expression"> a function without parameters returning something. </param>
    /// <param name="flag"> A flag that is used in dump. </param>
    /// <returns> the result of the invocation of the specified expression </returns>
    public static T Measure<T>
        (Func<T> expression, string flag = "") => Instance.InternalMeasure(expression, flag, 1);

    /// <summary>
    ///     Measures the specified action.
    /// </summary>
    /// <param name="action"> The action. </param>
    /// <param name="flag"> A flag that is used in dump. </param>
    public static void Measure(Action action, string flag = "") => Instance.InternalMeasure(action, flag, 1);

    /// <summary>
    ///     Resets the profiler data.
    /// </summary>
    public static void Reset()
    {
        lock(Instance)
            Instance.InternalReset();

        Instance = new();
    }

    /// <summary>
    ///     Formats the data accumulated so far.
    /// </summary>
    /// <param name="count"> The number of measured expressions in result, default is "null" for "no restriction. </param>
    /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
    /// <returns> The formatted data. </returns>
    /// <remarks>
    ///     The result contains one line for each measured expression, that is not ignored.
    ///     Each line contains
    ///     <para>
    ///         - the file path, the line and the start column of the measured expression in the source file, (The
    ///         information is formatted in a way, that within VisualStudio double clicking on such a line will open it.)
    ///     </para>
    ///     <para> - the flag, if provided, </para>
    ///     <para> - the ranking, </para>
    ///     <para> - the execution count, </para>
    ///     <para> - the average duration of one execution, </para>
    ///     <para> - the duration, </para>
    ///     of the expression.
    ///     The the lines are sorted by descending duration.
    ///     by use of <paramref name="count" /> and <paramref name="hidden" /> the number of lines can be restricted.
    /// </remarks>
    public static string Format(int? count = null, double hidden = 0.1)
    {
        lock(Instance)
            return new Dumper(Instance, count, hidden).Format();
    }

    void InternalMeasure(Action action, string flag, int stackFrameDepth)
    {
        BeforeAction(flag, stackFrameDepth + 1);
        action();
        AfterAction();
    }

    T InternalMeasure<T>(Func<T> expression, string flag, int stackFrameDepth)
    {
        BeforeAction(flag, stackFrameDepth + 1);
        var result = expression();
        AfterAction();
        return result;
    }

    void BeforeAction(string flag, int stackFrameDepth)
    {
        lock(this)
        {
            Stopwatch.Stop();
            var start = Stopwatch.Elapsed;
            Current.Suspend(start);
            Stack.Push(Current);
            var position = Tracer.MethodHeader(stackFrameDepth: stackFrameDepth + 1) + flag;
            if(!ProfileItems.TryGetValue(position, out Current))
            {
                Current = new(position);
                ProfileItems.Add(position, Current);
            }

            Current.Start(start);
            Stopwatch.Start();
        }
    }

    void AfterAction()
    {
        lock(this)
        {
            Stopwatch.Stop();
            var end = Stopwatch.Elapsed;
            Current.End(end);
            Current = Stack.Pop();
            Current.Resume(end);
            Stopwatch.Start();
        }
    }

    void InternalReset()
    {
        lock(this)
            (Stack.Count == 0).Assert();
    }
}

sealed class ProfileItem
{
    public TimeSpan Duration { get; private set; }
    readonly string Position;
    long CountEnd;
    long CountStart;
    long SuspendCount;

    public ProfileItem(string position) => Position = position;
    TimeSpan AverageDuration => new(Duration.Ticks / CountEnd);

    bool IsValid => CountStart == CountEnd && SuspendCount == 0;

    public void Start(TimeSpan duration)
    {
        CountStart++;
        Duration -= duration;
        if(IsValid)
            (Duration.Ticks >= 0).Assert();
    }

    public void End(TimeSpan duration)
    {
        CountEnd++;
        Duration += duration;
        if(IsValid)
            (Duration.Ticks >= 0).Assert();
    }

    public string Format(string tag)
    {
        IsValid.Assert();
        return Position +
            " #" +
            tag +
            ":  " +
            CountEnd.Format3Digits() +
            "target  " +
            AverageDuration.Format3Digits() +
            "  " +
            Duration.Format3Digits() +
            "\n";
    }

    public void Suspend(TimeSpan start)
    {
        SuspendCount++;
        Duration += start;
        if(IsValid)
            (Duration.Ticks >= 0).Assert();
    }

    public void Resume(TimeSpan end)
    {
        SuspendCount--;
        Duration -= end;
        if(IsValid)
            (Duration.Ticks >= 0).Assert();
    }
}