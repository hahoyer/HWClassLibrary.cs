using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class RightParenthesis : TokenClass
    {
        readonly int _level;
        public RightParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, Token token, ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            var leftParenthesisSyntax = left as LeftParenthesisSyntax;
            Tracer.Assert(leftParenthesisSyntax != null);
            Tracer.Assert(leftParenthesisSyntax.Right != null);
            Tracer.Assert(leftParenthesisSyntax.Level == _level);
            return leftParenthesisSyntax.Right;
        }
    }
}