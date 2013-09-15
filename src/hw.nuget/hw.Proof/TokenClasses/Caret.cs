using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class Caret : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new PowerSyntax(this, left, token, right);
        }
    }

    internal sealed class PowerSyntax : PairSyntax
    {
        public PowerSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        internal int CompareTo(PowerSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue) { return other.CombineForPlus(this, otherValue); }
        internal override ParsedSyntax CombineForPlus(ParsedSyntax other) { return other.CombineForPlus(this); }
        internal override ParsedSyntax CombineForPlus(PowerSyntax other, BigRational thisValue)
        {
            if (Left.CompareTo(other.Left) == 0 && Right.CompareTo(other.Right) == 0)
                return Times(thisValue + 1);
            return null;
        }

    }
}