using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class LeftParenthesis : ParserTokenType
    {
        readonly int Level;
        public LeftParenthesis(int level) { Level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            (left == null).Assert();
            return new LeftParenthesisSyntax(Level, token, right);
        }

        protected override string Id => Definitions.LeftBrackets[Level];
    }
}