using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

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
            AddCache = new FunctionCache<string, BracketContext>(GetAddCache);
            Brackets = brackets;
        }

        internal int Depth => Data.Length;

        protected override string GetNodeDump() => Data.Stringify("/");

        internal BracketContext Add(string token) => AddCache[token];

        internal bool? IsBracketAndLeftBracket(string token)
        {
            var delta = Depth - Add(token).Depth;
            return delta == 0? (bool?)null : delta < 0;
        }

        internal static BracketContext Instance(PrioTable.BracketPairItem[] brackets)
            => new BracketContext(new int[0], brackets);

        BracketContext GetAddCache(string token)
        {
            var index = GetContextIndex(token);
            if(index == 0)
                return this;

            var xx = Data
                .SkipWhile(item => item < 0 && item + index > 0)
                .ToArray();

            if(index <= 0)
                return new BracketContext
                    (new[] {index}.Concat(xx).ToArray(), Brackets);

            if(xx.FirstOrDefault() + index == 0)
                return new BracketContext(xx.Skip(1).ToArray(), Brackets);

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