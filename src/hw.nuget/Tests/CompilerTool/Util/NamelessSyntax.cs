using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class NamelessSyntax : TreeSyntax
    {
        public NamelessSyntax(Syntax left, IToken token, Syntax right)
            : base(left, token, right) { }

        public override string TokenClassName => "<nameless>";

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}