using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class RightParenthesis : ParserTokenType<ParsedSyntax>, IBracketMatch<ParsedSyntax>
    {
        readonly int Level;
        public RightParenthesis(int level) { Level = level; }

        protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            (right == null).Assert();
            var leftParenthesisSyntax = left as LeftParenthesisSyntax;
            (leftParenthesisSyntax != null).Assert();
            (leftParenthesisSyntax.Right != null).Assert();
            (leftParenthesisSyntax.Level == Level).Assert();
            return leftParenthesisSyntax.Right;
        }

        public override string Id => Definitions.RightBrackets[Level];
        IParserTokenType<ParsedSyntax> IBracketMatch<ParsedSyntax>.Value { get; } = new Matched();

        sealed class Matched : ParserTokenType<ParsedSyntax>
        {
            protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
                => right == null ? left : null;

            public override string Id => "()";
        }

    }
}