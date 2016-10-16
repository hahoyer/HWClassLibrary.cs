using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Integer : ParserTokenType
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);

            return new IntegerSyntax(token);
        }

        protected override string Id => "<integer>";
    }
}