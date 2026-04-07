namespace hw.Helper;

public readonly struct Range64(Index64 start, Index64? end = null)
{
    public Index64 Start { get; } = start;
    public Index64 End { get; } = end ?? new Index64(0, true);

    public Range ToInt => Start.ToInt..End.ToInt;

    public(long Offset, long Length) GetOffsetAndLength(long totalLength)
    {
        var s = Start.GetOffset(totalLength);
        var e = End.GetOffset(totalLength);
        if((ulong)e > (ulong)totalLength || (ulong)s > (ulong)e)
            throw new ArgumentOutOfRangeException();
        return (s, e - s);
    }

    public static Range64 All => new(0L, Index64.FromEnd(0));
    public override string ToString() => $"{Start}..{End}";
}