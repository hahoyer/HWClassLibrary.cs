using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class ErrorSyntax : TreeSyntax
    {
        readonly IssueId _message;

        public ErrorSyntax(Syntax left, IToken token, Syntax right, IssueId message)
            : base(left, token, right) { _message = message; }

        public override string TokenClassName => "?" + _message;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}