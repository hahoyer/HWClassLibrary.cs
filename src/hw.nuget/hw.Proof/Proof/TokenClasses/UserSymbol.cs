using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class UserSymbol : ParserTokenType
    {
        public UserSymbol(string value) { Id = value; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);

            return new VariableSyntax(token, Id);
        }

        protected override string Id { get; }
    }
}