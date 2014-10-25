using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class LeftParenthesis : TokenClass
    {
        readonly int _level;
        public LeftParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new LeftParenthesisSyntax(_level, token, right);
        }
    }
}