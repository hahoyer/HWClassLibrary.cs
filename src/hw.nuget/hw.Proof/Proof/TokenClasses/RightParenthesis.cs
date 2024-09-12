#nullable enable
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses;

sealed class RightParenthesis : ParserTokenType<ParsedSyntax>, IBracketMatch<ParsedSyntax>
{
    sealed class Matched : ParserTokenType<ParsedSyntax>
    {
        protected override ParsedSyntax? Create(ParsedSyntax? left, IToken token, ParsedSyntax? right)
            => right == null? left : null;

        public override string Id => "()";
    }

    readonly int Level;
    public RightParenthesis(int level) => Level = level;
    IParserTokenType<ParsedSyntax> IBracketMatch<ParsedSyntax>.Value { get; } = new Matched();

    protected override ParsedSyntax? Create(ParsedSyntax? left, IToken token, ParsedSyntax? right)
    {
        (right == null).Assert();
        var leftParenthesisSyntax = (left as LeftParenthesisSyntax).AssertNotNull();
        (leftParenthesisSyntax.Right != null).Assert();
        (leftParenthesisSyntax.Level == Level).Assert();
        return leftParenthesisSyntax.Right;
    }

    public override string Id => Definitions.RightBrackets[Level];
}