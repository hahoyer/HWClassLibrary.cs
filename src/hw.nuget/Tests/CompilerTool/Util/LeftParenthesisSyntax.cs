﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    [DebuggerDisplay("{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public LeftParenthesisSyntax
            (Syntax left, IToken part, Syntax right)
            : base(left, part, right) { }

        public override string TokenClassName => "<(>";
        public override bool TokenClassIsMain => false;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            if(right == null && Left == null)
                return new ParenthesisSyntax(Token, Right, token);

            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}