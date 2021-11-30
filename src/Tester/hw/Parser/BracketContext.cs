using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser
{
    public sealed class BracketContext : DumpableObject
    {
        readonly FunctionCache<string, BracketContext> AddCache;
        readonly PrioTable.BracketPairItem[] Brackets;

        [EnableDump]
        readonly int[] Data;

        BracketContext(int[] data, PrioTable.BracketPairItem[] brackets)
        {
            Data = data;
            AddCache = new(GetAddCache);
            Brackets = brackets;
        }

        protected override string GetNodeDump() => Data.Stringify("/");

        internal int Depth => Data.Length;

        internal BracketContext Add(string token) => AddCache[token];

        internal bool? IsLeftBracket(string token)
        {
            var delta = Depth - Add(token).Depth;
            return delta == 0? null : delta < 0;
        }

        internal static BracketContext Instance(PrioTable.BracketPairItem[] brackets) => new(new int[0], brackets);

        BracketContext GetAddCache(string token)
        {
            var index = GetContextIndex(token);
            if(index == 0)
                return this;

            var tail = Data
                .SkipWhile(item => item < 0 && item + index > 0)
                .ToArray();

            if(index <= 0)
                return new(new[] { index }.Concat(tail).ToArray(), Brackets);

            if(tail.FirstOrDefault() + index == 0)
                return new(tail.Skip(1).ToArray(), Brackets);

            return this;
        }

        int GetContextIndex(string token)
        {
            if(token == "")
                token = Depth == 0? PrioTable.BeginOfText : PrioTable.EndOfText;

            for(var index = 0; index < Brackets.Length; index++)
            {
                var item = Brackets[index];
                if(token == item.Left)
                    return -index - 1;
                if(token == item.Right)
                    return index + 1;
            }

            return 0;
        }
    }
}