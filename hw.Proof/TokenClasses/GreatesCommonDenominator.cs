using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class GreatesCommonDenominator : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new GreatesCommonDenominatorSyntax(this, left, token, right);
        }
    }

    internal sealed class GreatesCommonDenominatorSyntax : PairSyntax
    {
        public GreatesCommonDenominatorSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        internal int CompareTo(GreatesCommonDenominatorSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }
    }
}