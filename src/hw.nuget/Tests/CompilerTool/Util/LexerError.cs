using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    sealed class LexerError : DumpableObject,
        MatchExtension.IError
    {
        internal readonly IssueId IssueId;
        public LexerError(IssueId issueId) { IssueId = issueId; }
        public override string ToString() => IssueId.Tag;
    }
}