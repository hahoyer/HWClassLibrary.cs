using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof
{
    sealed class SyntaxError : ParserTokenType<ParsedSyntax>
    {
        [EnableDump]
        readonly IssueId _issueId;
        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string Id => PrioTable.Error;
    }
}