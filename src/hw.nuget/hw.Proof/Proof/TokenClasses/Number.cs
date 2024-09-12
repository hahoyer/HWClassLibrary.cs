using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class Number : ParserTokenType<ParsedSyntax>
    {
        protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null && right == null)
                return new NumberSyntax(token);

            NotImplementedMethod(left, token, right);
            return default!;
        }

        public override string Id => "";
    }
}