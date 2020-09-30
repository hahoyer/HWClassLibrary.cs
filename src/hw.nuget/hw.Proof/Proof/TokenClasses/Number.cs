using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Number : ParserTokenType<ParsedSyntax>
    {
        protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null && right == null)
                return new NumberSyntax(token);

            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string Id => "";
    }
}