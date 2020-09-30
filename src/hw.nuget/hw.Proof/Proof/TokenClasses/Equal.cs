using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Equal : PairToken
    {
        protected override string Id => "=";

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);

            return left.Equal(token, right);
        }

        protected override ParsedSyntax IsolateClause
            (string variable, ParsedSyntax left, ParsedSyntax right)
        {
            if(!left.Variables.Contains(variable))
            {
                if(right.Variables.Contains(variable))
                    return IsolateClause(variable, right, left);

                return null;
            }

            if(right.Variables.Contains(variable))
            {
                NotImplementedMethod(variable, left, right);
                return null;
            }

            return left.IsolateFromEquation(variable, right);
        }
    }
}