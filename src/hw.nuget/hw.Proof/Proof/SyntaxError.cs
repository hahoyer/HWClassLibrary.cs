using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IType<IParsedSyntax,TokenData>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        IParsedSyntax IType<IParsedSyntax, TokenData>.Create(IParsedSyntax left, TokenData part, IParsedSyntax right, bool isMatch)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        string IType<IParsedSyntax, TokenData>.PrioTableName { get { return PrioTable.Error; } }
        bool IType<IParsedSyntax, TokenData>.IsEnd { get { return false; } }
    }
}