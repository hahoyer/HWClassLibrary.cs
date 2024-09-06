using hw.Helper;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class IssueId
        : EnumEx
            , Match.IError
    {
        public static readonly IssueId EOFInComment = new();
        public static readonly IssueId EOFInLineComment = new();
        public static readonly IssueId EOLInString = new();
        public static readonly IssueId UnexpectedSyntaxError = new();
    }
}