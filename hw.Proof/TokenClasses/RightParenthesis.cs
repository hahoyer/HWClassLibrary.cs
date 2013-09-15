using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class RightParenthesis : TokenClass
    {
        private readonly int _level;
        public RightParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
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