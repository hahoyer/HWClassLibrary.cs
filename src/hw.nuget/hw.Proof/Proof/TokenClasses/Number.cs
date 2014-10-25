using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Number : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new NumberSyntax(token);
        }
    }
}