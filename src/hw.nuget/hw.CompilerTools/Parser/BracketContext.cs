using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Parser
{
    public sealed class BracketContext : DumpableObject
    {
        internal static readonly BracketContext Empty = new BracketContext(new int[] {});

        [EnableDump]
        readonly int[] Data;

        BracketContext(int[] data) { Data = data; }

        internal int Depth => Data.Length;

        public BracketContext Add(int index)
        {
            if(index == 0)
                return this;

            var xx = Data.SkipWhile(item => item < 0 && item + index > 0).ToArray();
            if(index > 0 && xx.FirstOrDefault() + index == 0)
                return new BracketContext(xx.Skip(1).ToArray());
            return new BracketContext(new[] {index}.Concat(xx).ToArray());
        }
    }
}