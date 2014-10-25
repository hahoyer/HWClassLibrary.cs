using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IType<Parser.ParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        Parser.ParsedSyntax IType<Parser.ParsedSyntax>.Create(Parser.ParsedSyntax left, SourcePart part, Parser.ParsedSyntax right, bool isMatch)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        string IType<Parser.ParsedSyntax>.PrioTableName { get { return PrioTable.Error; } }
        Control IType<Parser.ParsedSyntax>.Next { get { return null; } }
    }
}