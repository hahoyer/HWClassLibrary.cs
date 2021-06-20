using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool.Util
{
    sealed class SwitchToken : NamedToken, PrioParser<Syntax>.ISubParserProvider
    {
        public static readonly SwitchToken Instance = new SwitchToken();

        public SwitchToken()
            : base("-->") {}

        public override bool IsMain => true;

        protected override Syntax Create(Syntax left, IToken token, Syntax right) => null;

        static IParserTokenType<Syntax> Converter(Syntax arg) => new SyntaxBoxToken(arg);

        sealed class SyntaxBoxToken : ParserTokenType<Syntax>
        {
            [EnableDump]
            readonly Syntax Content;
            public SyntaxBoxToken(Syntax content) { Content = content; }
            public override string Id => "<box>";

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(left == null);
                Tracer.Assert(right == null);
                return Content;
            }
        }

        ISubParser<Syntax> PrioParser<Syntax>.ISubParserProvider.NextParser
            => NestedTokenFactory.Parser.Convert(Converter);
    }
}