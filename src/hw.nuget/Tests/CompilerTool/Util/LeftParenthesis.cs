﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class LeftParenthesis : ParserTokenType<Syntax>
    {
        public LeftParenthesis(string id) { Id = id; }
        public override string Id { get; }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new LeftParenthesisSyntax(left, token, right);
    }
}