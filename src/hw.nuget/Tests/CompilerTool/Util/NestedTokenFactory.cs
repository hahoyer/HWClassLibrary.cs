using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Parser =
            new NestedTokenFactory().ParserInstance;

        NestedTokenFactory() { }

        protected override PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.BracketParallels
                    (LeftBrackets, RightBrackets);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        internal override IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses
            => new TokenClass<Syntax>[]
            {
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
            };

        internal override TokenClass<Syntax> GetTokenClass(string name) => new NestedToken(name);
    }
}