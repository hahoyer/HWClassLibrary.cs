using hw.DebugFormatter;
using hw.Parser;

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
                Tracer.Assert(!Right.Variables.Contains(variable));
                return Left.IsolateFromEquation(variable, Right);
            }
            Tracer.Assert(Right.Variables.Contains(variable));
            return Right.IsolateFromEquation(variable, Left);
        }
    }
}