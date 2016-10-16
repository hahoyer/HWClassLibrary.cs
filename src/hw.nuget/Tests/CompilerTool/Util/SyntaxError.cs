using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class SyntaxError : CommonTokenType<Syntax>
    {
        [EnableDump]
        readonly IssueId IssueId;
        public SyntaxError(IssueId issueId) { IssueId = issueId; }
        protected override string Id => PrioTable.Error;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new ErrorSyntax(left, token, right, IssueId);
    }
}