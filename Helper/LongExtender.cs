using System;
using System.Linq;
using System.Collections.Generic;

namespace HWClassLibrary.Helper
{
    static public class LongExtender
    {
        public static string Format3Digits(this long size)
        {
            var i = 0;
            for (; size >= 1000; i++, size /= 10)
                continue;

            var result = size.ToString();
            switch (i % 3)
            {
                case 1:
                    result = result.Insert(1, ".");
                    break;
                case 2:
                    result = result.Insert(2, ".");
                    break;
            }

            if (i == 0)
                return result;
            return result + "kMGT"[(i - 1) / 3];
        }
    }
}