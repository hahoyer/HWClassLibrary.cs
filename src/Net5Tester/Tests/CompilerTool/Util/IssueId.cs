﻿using hw.Helper;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool.Util
{
    sealed class IssueId
        : EnumEx
            , Match.IError
    {
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId UnexpectedSyntaxError = new IssueId();
    }
}