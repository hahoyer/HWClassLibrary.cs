using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Proof.TokenClasses;

namespace hw.Proof
{
    sealed class SyntaxError : CommonTokenType
    {
        [EnableDump]
        readonly IssueId _issueId;
        public SyntaxError(IssueId issueId) { _issueId = issueId; }
        protected override string Id => PrioTable.Error;
    }
}