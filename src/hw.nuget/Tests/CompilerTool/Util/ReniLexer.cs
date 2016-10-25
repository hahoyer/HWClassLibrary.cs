using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    sealed class Lexer : Match2TwoLayerScannerGuard
    {
        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly IssueId _invalidTextEnd = IssueId.EOLInString;
        readonly IssueId _invalidLineComment = IssueId.EOFInLineComment;
        readonly IssueId _invalidComment = IssueId.EOFInComment;
        readonly IMatch _number;
        readonly Match _comment;

        public Lexer()
            : base(error => new SyntaxError((IssueId) error))
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

        internal int? WhiteSpace(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _whiteSpaces);
        internal int? Comment(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _comment);
        internal int? Number(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _number);
        internal int? Any(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _any);
        internal int? Text(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, _text);
    }
}