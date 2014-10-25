using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    sealed class Scanner : hw.Parser.Scanner
    {
        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly SyntaxError _invalidTextEnd = new SyntaxError(IssueId.EOLInString);
        readonly SyntaxError _invalidLineComment = new SyntaxError(IssueId.EOFInLineComment);
        readonly SyntaxError _invalidComment = new SyntaxError(IssueId.EOFInComment);
        readonly SyntaxError _unexpectedSyntaxError = new SyntaxError(IssueId.UnexpectedSyntaxError);
        readonly IMatch _number;

        public Scanner()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _whiteSpaces =
                Match.WhiteSpace.Else("#" + " \t".AnyChar() + Match.LineEnd.Find)
                    .Else("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else("#(" + _any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else("#(" + Match.End.Find + _invalidComment)
                    .Else("#" + Match.End.Find + _invalidLineComment)
                    .Repeat();

            _number = Match.Digit.Repeat(1);

            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(Match.LineEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    });
        }

        protected override int WhiteSpace(SourcePosn sourcePosn)
        {
            var result = ExceptionGuard(sourcePosn, _whiteSpaces);
            Tracer.Assert(result != null);
            return result.Value;
        }

        protected override int? Number(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _number); }
        protected override int? Any(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _any); }
        protected override int? Text(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _text); }

        int? ExceptionGuard(SourcePosn sourcePosn, IMatch match)
        {
            try
            {
                return sourcePosn.Match(match);
            }
            catch(Match.Exception exception)
            {
                throw new Exception
                    (sourcePosn, exception.Error as SyntaxError ?? _unexpectedSyntaxError, exception.SourcePosn - sourcePosn);
            }
        }
    }

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

    sealed class IssueId : EnumEx
    {
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId UnexpectedSyntaxError = new IssueId();
    }
}