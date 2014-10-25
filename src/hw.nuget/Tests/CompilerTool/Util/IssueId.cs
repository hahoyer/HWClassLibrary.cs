using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace hw.Tests.CompilerTool.Util
{
    sealed class IssueId : EnumEx
    {
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId UnexpectedSyntaxError = new IssueId();
    }
}