namespace hw.Helper;

public readonly struct Index64(long value, bool fromEnd = false)
{
    public long Value { get; } = value;
    public bool IsFromEnd { get; } = fromEnd;

    public Index ToInt => checked((int)Value);

    public long GetOffset(long length) => IsFromEnd? length - Value : Value;

    public static implicit operator Index64(long value) => new(value);
    public static Index64 FromEnd(long value) => new(value, true);

    public override string ToString() => IsFromEnd? $"^{Value}" : Value.ToString();

    public static Range64 operator /(Index64 start, Index64? end) => new(start, end);
    public static Range64 operator /(Index64 start, long end) => new(start, end);
    public static Range64 operator /(long start, Index64 end) => new(start, end);
}