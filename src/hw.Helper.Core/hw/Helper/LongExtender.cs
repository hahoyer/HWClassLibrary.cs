// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>Numeric helpers around <see cref="long" /> and <see cref="int" />.</summary>
[PublicAPI]
public static class LongExtender
{
    /// <summary>10^9 as a <see cref="long" /> (decimal giga, not 2^30).</summary>
    public static long Giga => 1000 * 1000 * 1000;

    /// <summary>10^6 as a <see cref="long" /> (decimal mega, not 2^20).</summary>
    public static long Mega => 1000 * 1000;

    /// <summary>10^3 as a <see cref="long" />.</summary>
    public static long Kilo => 1000;

    /// <summary>
    ///     Formats <paramref name="value" /> with at most three significant digits and an SI suffix
    ///     (k, M, G, T, P, E, Z, Y), e.g. <c>1234567 → "1.23M"</c>. Negative numbers keep their sign.
    /// </summary>
    /// <param name="value"> Value to format. </param>
    /// <param name="omitZeros"> If <c>true</c>, trailing fractional zeros are dropped. </param>
    public static string Format3Digits(this long value, bool omitZeros = true)
    {
        if(value == 0)
            return "0";
        if(value < 0)
            return "-" + (-value).Format3Digits(omitZeros);

        var size = 0;
        for(; value >= 1000; size++, value /= 10) { }

        var result = value.ToString();
        var startIndex = size % 3;
        if(startIndex > 0)
            if(omitZeros && result.Skip(startIndex).All(c => c == '0'))
                result = new(result.Take(startIndex).ToArray());
            else
            {
                result = result.Insert(startIndex, ".");
                if(omitZeros)
                    while(result.Last() == '0')
                        result = result.Substring(0, result.Length - 1);
            }

        if(size == 0)
            return result;
        // ReSharper disable once StringLiteralTypo
        return result + "kMGTPEZY"[(size - 1) / 3];
    }

    /// <summary>
    ///     Tests whether every bit set in <paramref name="bit" /> is also set in
    ///     <paramref name="bitArray" /> (i.e. a bitmask containment check, not single-bit equality).
    /// </summary>
    public static bool HasBitSet(this long bitArray, long bit) => (bitArray & bit) == bit;

    /// <inheritdoc cref="HasBitSet(long, long)" />
    public static bool HasBitSet(this int bitArray, int bit) => (bitArray & bit) == bit;
}