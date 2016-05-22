using System;
using System.Collections.Generic;
using hw.Scanner;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class ReniLexer : ILexer
    {
        internal sealed class Error : Match.IError
        {
            public readonly IssueId IssueId;
            public Error(IssueId issueId) { IssueId = issueId; }
            public override string ToString() { return IssueId.Tag; }
        }

        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly Error _invalidTextEnd = new Error(IssueId.EOLInString);
        readonly Error _invalidLineComment = new Error(IssueId.EOFInLineComment);
        readonly Error _invalidComment = new Error(IssueId.EOFInComment);
        readonly IMatch _number;
        readonly Match _comment;

        public ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _whiteSpaces = Match.WhiteSpace.Repeat(1);

            _comment =
                ("#" + " \t".AnyChar() + Match.LineEnd.Find)
                    .Else("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else("#(" + _any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else("#(" + Match.End.Find + _invalidComment)
                    .Else("#" + Match.End.Find + _invalidLineComment)
                ;

            _number = Match.Digit.Repeat(1);

            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(Match.LineEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    });
        }

        Func<SourcePosn, int?>[] ILexer.WhiteSpace
        {
            get
            {
                return new Func<SourcePosn, int?>[]
                {
                    sourcePosn => sourcePosn.Match(_whiteSpaces),
                    sourcePosn => sourcePosn.Match(_comment)
                };
            }
        }

        int? ILexer.Number(SourcePosn sourcePosn) { return sourcePosn.Match(_number); }
        int? ILexer.Any(SourcePosn sourcePosn) { return sourcePosn.Match(_any); }

        Match.IError ILexer.InvalidCharacterError { get; } = new Error(IssueId.UnexpectedSyntaxError);

        int? ILexer.Text(SourcePosn sourcePosn) { return sourcePosn.Match(_text); }
    }
}