using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    [DebuggerDisplay("{NodeDump}")]
    sealed class RightParenthesisSyntax : TreeSyntax
    {
        readonly string Id;

        public RightParenthesisSyntax(string id, Syntax left, IToken part, Syntax right)
            : base(left, part, right) { Id = id; }

        public override string TokenClassName => "?)?";
        public override bool TokenClassIsMain => false;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}