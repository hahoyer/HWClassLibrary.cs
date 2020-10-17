using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class Integer : ParserTokenType
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);

            return new IntegerSyntax(token);
        }

        protected override string Id => "<integer>";
    }
}