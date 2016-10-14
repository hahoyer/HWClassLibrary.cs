using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    sealed class ReniLexer : ILexer
    {
        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly LexerError _invalidTextEnd = new LexerError(IssueId.EOLInString);
        readonly LexerError _invalidLineComment = new LexerError(IssueId.EOFInLineComment);
        readonly LexerError _invalidComment = new LexerError(IssueId.EOFInComment);
        readonly IMatch _number;
        readonly Match _comment;

        public ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));

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
            => new Func<SourcePosn, int?>[]
            {
                sourcePosn => sourcePosn.Match(_whiteSpaces),
                sourcePosn => sourcePosn.Match(_comment)
            };

        static int? GuardedMatch(SourcePosn sourcePosn, IMatch match)
        {
            try
            {
                return sourcePosn.Match(match);
            }
            catch(Exception systemException)
            {
                var exception = systemException as MatchExtension.IException;
                if(exception == null)
                    throw;

                throw new LexerException(sourcePosn.Span(exception.SourcePosn), ((LexerError)exception.Error).IssueId);
            }
        }

        sealed class LexerException : Exception
        {
            readonly SourcePart Position;
            readonly IssueId IssueId;

            public LexerException(SourcePart position, IssueId issueId)
            {
                Position = position;
                IssueId = issueId;
            }
        }

        int? ILexer.Number(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _number);
        int? ILexer.Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
        int? ILexer.Text(SourcePosn sourcePosn) => sourcePosn.Match(_text);
    }
}