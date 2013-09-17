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
        public static string Format3Digits(this long value)
        {
            var size = 0;
            for(; value >= 1000; size++, value /= 10)
                continue;

            var result = value.ToString();
            if(size % 3 > 0)
                result = result.Insert(size % 3, ".");

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
    }

    [TestFixture]
    public sealed class TestLongExtender
    {
        [Test]
        public void TestMethod()
        {
            InternalTest(1, "1");
            InternalTest(12, "12");
            InternalTest(123, "123");
            InternalTest(1234, "1.23k");
            InternalTest(12345, "12.3k");
            InternalTest(123456, "123k");
            InternalTest(1234567, "1.23M");
            InternalTest(12345678, "12.3M");
            InternalTest(123456789, "123M");
            InternalTest(1234567890, "1.23G");
            InternalTest(12345678901, "12.3G");
            InternalTest(123456789012, "123G");
            InternalTest(1234567890123, "1.23T");
            InternalTest(12345678901234, "12.3T");
            InternalTest(123456789012345, "123T");
            InternalTest(1234567890123456, "1.23P");
            InternalTest(12345678901234567, "12.3P");
            InternalTest(123456789012345678, "123P");
        }

        static void InternalTest(long x, string y) { Tracer.Assert(x.Format3Digits() == y, () => x + " != " + y, 1); }
    }
}