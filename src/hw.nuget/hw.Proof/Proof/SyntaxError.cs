﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class SyntaxError : Dumpable, IType<ParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        ParsedSyntax IType<ParsedSyntax>.Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
        string IType<ParsedSyntax>.PrioTableId { get { return PrioTable.Error; } }
        IType<ParsedSyntax> IType<ParsedSyntax>.NextTypeIfMatched { get { return null; } }
    }
}