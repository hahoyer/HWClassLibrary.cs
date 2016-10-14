using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IParserType<ParsedSyntax>
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        ParsedSyntax IParserType<ParsedSyntax>.Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
        string IParserType<ParsedSyntax>.PrioTableId { get { return PrioTable.Error; } }

    }
}