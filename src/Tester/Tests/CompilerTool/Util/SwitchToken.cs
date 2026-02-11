using hw.Parser;

namespace Tester.Tests.CompilerTool.Util
{
    sealed class SwitchToken : NamedToken, PrioParser<Syntax>.ISubParserProvider
    {
        public static readonly SwitchToken Instance = new SwitchToken();

        public SwitchToken()
            : base("-->") {}

        public override bool IsMain => true;

        protected override Syntax? Create(Syntax? left, IToken token, Syntax? right) => null;

        static IParserTokenType<Syntax> Converter(Syntax arg) => new SyntaxBoxToken(arg);

        sealed class SyntaxBoxToken : ParserTokenType<Syntax>
        {
            [EnableDump]
            readonly Syntax Content;
            public SyntaxBoxToken(Syntax content) => Content = content;
            public override string Id => "<box>";

            protected override Syntax Create(Syntax? left, IToken token, Syntax? right)
            {
                (left == null).Assert();
                (right == null).Assert();
                return Content;
            }
        }

        ISubParser<Syntax> PrioParser<Syntax>.ISubParserProvider.NextParser
            => NestedTokenFactory.Parser.Convert(Converter);
    }
}