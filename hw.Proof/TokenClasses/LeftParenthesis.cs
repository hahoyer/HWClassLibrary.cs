using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class LeftParenthesis : TokenClass
    {
        private readonly int _level;
        public LeftParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new LeftParenthesisSyntax(_level, token, right);
        }
    }
}