#pragma warning disable IDE0130
namespace hw.Helper;

/// <summary>
///     Long-typed counterpart to <see cref="Index" />: an absolute or end-relative position into a
///     sequence whose length exceeds <see cref="int.MaxValue" />. Use <c>/</c> to combine two indices
///     into a <see cref="Range64" />.
/// </summary>
/// <param name="value"> Distance from the start (or from the end if <paramref name="fromEnd" /> is <c>true</c>). </param>
/// <param name="fromEnd"> If <c>true</c>, <paramref name="value" /> is measured from the end. </param>
public readonly struct Index64(long value, bool fromEnd = false)
{
    /// <summary>Magnitude of the index (always non-negative).</summary>
    public long Value { get; } = value;

    /// <summary>Whether <see cref="Value" /> is measured from the end.</summary>
    public bool IsFromEnd { get; } = fromEnd;

    /// <summary>Narrowing conversion to <see cref="Index" />; throws on overflow.</summary>
    public Index ToInt => checked((int)Value);

    /// <summary>Resolves the index against a sequence of <paramref name="length" /> items.</summary>
    public long GetOffset(long length) => IsFromEnd? length - Value : Value;

    /// <summary>Implicit lift from <see cref="long" /> as an absolute index.</summary>
    public static implicit operator Index64(long value) => new(value);

    /// <summary>Constructs an end-relative index, e.g. <c>FromEnd(0)</c> denotes the position past the last element.</summary>
    public static Index64 FromEnd(long value) => new(value, true);

    /// <summary>Renders absolute indices as digits and end-relative ones with a leading <c>^</c>.</summary>
    public override string ToString() => IsFromEnd? $"^{Value}" : Value.ToString();

    /// <summary>Combines two indices into a <see cref="Range64" />.</summary>
    public static Range64 operator /(Index64 start, Index64? end) => new(start, end);

    /// <inheritdoc cref="op_Division(Index64, Index64?)" />
    public static Range64 operator /(Index64 start, long end) => new(start, end);

    /// <inheritdoc cref="op_Division(Index64, Index64?)" />
    public static Range64 operator /(long start, Index64 end) => new(start, end);
}