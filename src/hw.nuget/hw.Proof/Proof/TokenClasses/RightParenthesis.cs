using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class RightParenthesis : CommonTokenType, IBracketMatch<ParsedSyntax>
    {
        readonly int _level;
        public RightParenthesis(int level) { _level = level; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            var leftParenthesisSyntax = left as LeftParenthesisSyntax;
            Tracer.Assert(leftParenthesisSyntax != null);
            Tracer.Assert(leftParenthesisSyntax.Right != null);
            Tracer.Assert(leftParenthesisSyntax.Level == _level);
            return leftParenthesisSyntax.Right;
        }

        protected override string Id => Definitions.RightBrackets[_level];
        IParserTokenType<ParsedSyntax> IBracketMatch<ParsedSyntax>.Value { get; } = new Matched();

        sealed class Matched : TokenClass<ParsedSyntax>
        {
            protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
                => right == null ? left : null;

            public override string Id => "()";
        }

    }
}