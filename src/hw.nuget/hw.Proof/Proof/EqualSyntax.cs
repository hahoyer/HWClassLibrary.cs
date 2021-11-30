using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class EqualSyntax : PairSyntax, IComparableEx<EqualSyntax>
    {
        public EqualSyntax(ParsedSyntax left, IToken token, ParsedSyntax right)
            : base(Definitions.Equal, left, token, right) { }

        int IComparableEx<EqualSyntax>.CompareToEx(EqualSyntax other)
        {
            var result = Left.CompareTo(other.Right);
            if(result == 0)
                return Right.CompareTo(other.Left);

            result = Left.CompareTo(other.Left);
            if(result == 0)
                return Right.CompareTo(other.Right);

            return result;
        }

        protected override ParsedSyntax IsolateClause(string variable)
        {
            if(Left.Variables.Contains(variable))
            {
                (!Right.Variables.Contains(variable)).Assert();
                return Left.IsolateFromEquation(variable, Right);
            }
            Right.Variables.Contains(variable).Assert();
            return Right.IsolateFromEquation(variable, Left);
        }
    }
}