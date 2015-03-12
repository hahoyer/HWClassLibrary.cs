using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class Integer : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new IntegerSyntax(token);
        }
        public override string Value { get { return "<integer>"; } }
    }
}