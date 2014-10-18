using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IType<IParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart part, IParsedSyntax right, bool isMatch)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        string IType<IParsedSyntax>.PrioTableName { get { return PrioTable.Error; } }
        bool IType<IParsedSyntax>.IsEnd { get { return false; } }
    }
}