using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class LeftParenthesisSyntax : ParsedSyntax
    {
        internal readonly int Level;
        internal readonly ParsedSyntax Right;

        public LeftParenthesisSyntax(int level, Token token, ParsedSyntax right)
            : base(token)
        {
            Level = level;
            Right = right;
        }
    }
}