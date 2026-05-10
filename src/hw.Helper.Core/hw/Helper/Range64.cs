#pragma warning disable IDE0130
namespace hw.Helper;

/// <summary>
///     Long-typed counterpart to <see cref="Range" /> over a pair of <see cref="Index64" /> values.
///     The end is exclusive; if not supplied it defaults to <see cref="Index64.FromEnd" /> with
///     value 0, i.e. "to the end".
/// </summary>
/// <param name="start"> Inclusive start index. </param>
/// <param name="end"> Exclusive end index; <c>null</c> means "to the end". </param>
public readonly struct Range64(Index64 start, Index64? end = null)
{
    /// <summary>Inclusive start of the range.</summary>
    public Index64 Start { get; } = start;

    /// <summary>Exclusive end of the range.</summary>
    public Index64 End { get; } = end ?? new Index64(0, true);

    /// <summary>Narrowing conversion to <see cref="Range" />; throws on overflow.</summary>
    public Range ToInt => Start.ToInt..End.ToInt;

    /// <summary>
    ///     Resolves the range against a sequence of <paramref name="totalLength" /> items, returning
    ///     the absolute start offset and the slice length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> The resolved range falls outside <c>[0, <paramref name="totalLength" />]</c>. </exception>
    public(long Offset, long Length) GetOffsetAndLength(long totalLength)
    {
        var s = Start.GetOffset(totalLength);
        var e = End.GetOffset(totalLength);
        if((ulong)e > (ulong)totalLength || (ulong)s > (ulong)e)
            throw new ArgumentOutOfRangeException();
        return (s, e - s);
    }

    /// <summary>The full range (start through end of the sequence).</summary>
    public static Range64 All => new(0L, Index64.FromEnd(0));

    /// <inheritdoc />
    public override string ToString() => $"{Start}..{End}";
}