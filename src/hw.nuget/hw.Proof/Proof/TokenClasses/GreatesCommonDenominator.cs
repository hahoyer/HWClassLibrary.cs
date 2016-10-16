using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class GreatesCommonDenominator : PairToken
    {
        protected override string Id => "gcd";

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);

            return new GreatesCommonDenominatorSyntax(this, left, token, right);
        }
    }

    sealed class GreatesCommonDenominatorSyntax : PairSyntax
    {
        public GreatesCommonDenominatorSyntax
            (IPair @operator, ParsedSyntax left, IToken token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        internal int CompareTo(GreatesCommonDenominatorSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }
    }
}