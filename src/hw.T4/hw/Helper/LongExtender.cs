using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    public static class LongExtender
    {
        public static string Format3Digits(this long value, bool omitZeros = true)
        {
            if(value == 0)
                return "0";
            if(value < 0)
                return "-" + Format3Digits(-value, omitZeros);

            var size = 0;
            for(; value >= 1000; size++, value /= 10)
                continue;

            var result = value.ToString();
            var startIndex = size % 3;
            if(startIndex > 0)
                if(omitZeros && result.Skip(startIndex).All(c => c == '0'))
                    result = new string(result.Take(startIndex).ToArray());
                else
                {
                    result = result.Insert(startIndex, ".");
                    if(omitZeros)
                        while(result.Last() == '0')
                            result = result.Substring(0, result.Length - 1);
                }

            if(size == 0)
                return result;
            return result + "kMGTPEZY"[(size - 1) / 3];
        }
        /// <summary>
        ///     Gets the giga.
        /// </summary>
        /// <value>The giga.</value>
        /// [created 30.08.2006 22:24]
        public static long Giga { get { return 1000 * 1000 * 1000; } }

        /// <summary>
        ///     Gets the mega.
        /// </summary>
        /// <value>The mega.</value>
        /// [created 30.08.2006 22:24]
        public static long Mega { get { return 1000 * 1000; } }

        /// <summary>
        ///     Gets the kilo.
        /// </summary>
        /// <value>The kilo.</value>
        /// [created 30.08.2006 22:24]
        public static long Kilo { get { return 1000; } }

        public static bool HasBitSet(this long bitArray, long bit) { return (bitArray & bit) == bit; }
        public static bool HasBitSet(this int bitArray, int bit) { return (bitArray & bit) == bit; }
    }
}