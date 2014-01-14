#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace hw.Helper
{
    public static class LongExtender
    {
        public static string Format3Digits(this long value, bool omitZeros=true)
        {
            var size = 0;
            for(; value >= 1000; size++, value /= 10)
                continue;

            var result = value.ToString();
            var decimalPointIndex = size % 3; 
            if (decimalPointIndex > 0)
            {
                if (omitZeros)
                    while(result.Last() == '0' && decimalPointIndex < result.Length)
                        result = result.Substring(0, result.Length - 1);
                if(decimalPointIndex < result.Length)
                    result = result.Insert(decimalPointIndex, ".");
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