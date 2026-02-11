using hw.Scanner;

namespace Tester.Tests.CompilerTool.Util
{
    [PublicAPI]
    sealed class Lexer : Match2TwoLayerScannerGuard
    {
        readonly Match WhiteSpaces;
        readonly Match AnyMatch;
        readonly Match TextMatch;
        readonly IssueId InvalidTextEnd = IssueId.EOLInString;
        readonly IssueId InvalidLineComment = IssueId.EOFInLineComment;
        readonly IssueId InvalidComment = IssueId.EOFInComment;
        readonly IMatch NumberMatch;
        readonly Match CommentMatch;

        public Lexer()
            : base(error => new SyntaxError((IssueId) error))
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));

            AnyMatch = symbol1.Else(identifier);

            WhiteSpaces = Match.WhiteSpace.Repeat(1);

            CommentMatch =
                ("#" + " \t".AnyChar() + Match.LineEnd.Find)
                    .Else("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else("#(" + AnyMatch.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else("#(" + Match.End.Find + InvalidComment)
                    .Else("#" + Match.End.Find + InvalidLineComment)
                ;

            NumberMatch = Match.Digit.Repeat(1);

            TextMatch = textFrame.Value
            (
                head =>
                {
                    var textEnd = head.Else(Match.LineEnd + InvalidTextEnd);
                    return textEnd.Find + (head + textEnd.Find).Repeat();
                });
        }

        internal int? Space(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, WhiteSpaces);
        internal int? Comment(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, CommentMatch);
        internal int? Number(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, NumberMatch);
        internal int? Any(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, AnyMatch);
        internal int? Text(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, TextMatch);
    }
}