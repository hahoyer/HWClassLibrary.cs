using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Is used by prio parser to manage the bracket level during parsing process
/// </summary>
public sealed class BracketContext : DumpableObject
{
    readonly FunctionCache<string, BracketContext> AddCache;
    readonly PrioTable.BracketPairItem[] Brackets;

    /// <summary>
    ///     contains the codes of open brackets. It is the negative index of <see cref="Brackets" /> member, minus 1.
    ///     So the different bracket-pairs have a priority itself
    ///     that is used in stream recovery
    ///     when brackets are not evenly paired.
    /// </summary>
    [EnableDump]
    readonly int[] Data;

    /// <summary>
    ///     Actually the number of open brackets
    /// </summary>
    internal int Depth => Data.Length;

    BracketContext(int[] data, PrioTable.BracketPairItem[] brackets)
    {
        Data = data;
        AddCache = new(GetAddCache);
        Brackets = brackets;
    }

    protected override string GetNodeDump() => Data.Stringify("/");

    /// <summary>
    ///     Create the starting context from priority table
    /// </summary>
    /// <param name="brackets"></param>
    /// <returns></returns>
    internal static BracketContext Start(PrioTable.BracketPairItem[] brackets) => new([], brackets);

    BracketContext Add(string token) => AddCache[token];

    internal BracketSide IsLeftBracket(string token)
    {
        var delta = Depth - Add(token).Depth;
        return delta == 0? BracketSide.None :
            delta < 0? BracketSide.Left : BracketSide.Right;
    }

    BracketContext GetAddCache(string token)
    {
        var index = GetContextIndex(token);
        if(index == 0)
            return this;

        var tail = Data
            .SkipWhile(item => item < 0 && item + index > 0)
            .ToArray();

        if(index < 0)
            return new(new[] { index }.Concat(tail).ToArray(), Brackets);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        (index > 0).Assert();

        return tail.FirstOrDefault() + index == 0
            ? new(tail.Skip(1).ToArray(), Brackets)
            : this;
    }

    int GetContextIndex(string token)
    {
        if(token == "")
            token = Depth == 0? PrioTable.BeginOfText : PrioTable.EndOfText;

        for(var index = 0; index < Brackets.Length; index++)
        {
            var item = Brackets[index];
            if(token == item.Left)
                return -index - 1;
            if(token == item.Right)
                return index + 1;
        }

        return 0;
    }

    /// <summary>
    ///     Create the new context after the scanner has advanced
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal static BracketContext GetRightContext(PrioTable.ITargetItem item)
        => item.LeftContext.Add(item.Token);

    /// <summary>
    ///     Get the new bracket depth after the scanner has advanced
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal static int GetRightDepth(PrioTable.ITargetItem item)
        => GetRightContext(item).Depth;
}