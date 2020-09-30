using hw.Helper;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Caret : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);

            return new PowerSyntax(this, left, token, right);
        }

        protected override string Id => "^";
    }

    sealed class PowerSyntax : PairSyntax
    {
        public PowerSyntax(IPair @operator, ParsedSyntax left, IToken token, ParsedSyntax right)
            : base(@operator, left, token, right) {}

        internal int CompareTo(PowerSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue)
            => other.CombineForPlus(this, otherValue);

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other)
            => other.CombineForPlus(this);

        internal override ParsedSyntax CombineForPlus(PowerSyntax other, BigRational thisValue)
        {
            if(Left.CompareTo(other.Left) == 0 && Right.CompareTo(other.Right) == 0)
                return Times(thisValue + 1);

            return null;
        }
    }
}