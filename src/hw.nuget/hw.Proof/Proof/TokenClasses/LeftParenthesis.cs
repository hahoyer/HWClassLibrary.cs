using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class LeftParenthesis : ParserTokenType
    {
        readonly int _level;
        public LeftParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new LeftParenthesisSyntax(_level, token, right);
        }

        protected override string Id => Definitions.LeftBrackets[_level];
    }
}