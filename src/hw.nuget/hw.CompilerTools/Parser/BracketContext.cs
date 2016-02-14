using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.Parser
{
    public sealed class BracketContext : DumpableObject
    {
        internal static readonly BracketContext Empty = new BracketContext(new int[] {});

        readonly FunctionCache<int, BracketContext> AddCache;

        [EnableDump]
        readonly int[] Data;

        BracketContext(int[] data)
        {
            Data = data;
            AddCache = new FunctionCache<int, BracketContext>(GetAddCache);
        }

        protected override string GetNodeDump() => Data.Stringify("/");

        BracketContext GetAddCache(int index)
        {
            if (index == 0)
                return this;

            var xx = Data
                .SkipWhile(item => item < 0 && item + index > 0)
                .ToArray();

            if(index <= 0)
                return new BracketContext(new[] {index}.Concat(xx).ToArray());

            if (xx.FirstOrDefault() + index == 0)
                return new BracketContext(xx.Skip(1).ToArray());

            return this;
        }

        internal int Depth => Data.Length;

        public BracketContext Add(int index) => AddCache[index];
    }
}