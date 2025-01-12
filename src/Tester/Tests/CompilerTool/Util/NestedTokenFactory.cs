using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

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
                var target = PrioTable.Left(PrioTable.Any);
                target += PrioTable.BracketParallels
                    (LeftBrackets, RightBrackets);
                ("\n" + target.Dump() + "\n").FlaggedLine();
                target.Title = Tracer.MethodHeader();
                return target;
            }
        }

        internal override IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses
            => new ParserTokenType<Syntax>[]
            {
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
            };

        internal override ParserTokenType<Syntax> GetTokenClass(string name) => new NestedToken(name);
    }
}