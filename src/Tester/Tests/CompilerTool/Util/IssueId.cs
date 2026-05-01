using hw.Scanner;

namespace Tester.Tests.CompilerTool.Util
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
