using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IType<ParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        ParsedSyntax IType<ParsedSyntax>.Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        IType<ParsedSyntax> IType<ParsedSyntax>.Match(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return null;
        }
        string IType<ParsedSyntax>.PrioTableName { get { return PrioTable.Error; } }
        ISubParser<ParsedSyntax> IType<ParsedSyntax>.Next { get { return null; } }
    }
}