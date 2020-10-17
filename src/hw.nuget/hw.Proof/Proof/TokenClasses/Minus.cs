using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class Minus : PairToken
    {
        protected override string Id => "-";

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);

            return left.Minus(token, right);
        }
    }
}