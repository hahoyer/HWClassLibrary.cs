#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ReniScanner : Scanner
    {
        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly SyntaxError _invalidTextEnd = new SyntaxError(IssueId.EOLInString);
        readonly SyntaxError _invalidLineComment = new SyntaxError(IssueId.EOFInLineComment);
        readonly SyntaxError _invalidComment = new SyntaxError(IssueId.EOFInComment);
        readonly SyntaxError _unexpectedSyntaxError = new SyntaxError(IssueId.UnexpectedSyntaxError);
        readonly IMatch _number;

        public ReniScanner()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier =
                (alpha + (alpha.Else(Match.Digit)).Repeat())
                    .Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _whiteSpaces = Match.WhiteSpace
                .Else("#" + " \t".AnyChar() + Match.LineEnd.Find)
                .Else("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                .Else("#(" + _any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                .Else("#(" + Match.End.Find + _invalidComment)
                .Else("#" + Match.End.Find + _invalidLineComment)
                .Repeat();

            _number = Match.Digit.Repeat(1);

            _text = textFrame
                .Value
                (head                       
                 =>
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
                    (sourcePosn
                     , exception.Error as SyntaxError ?? _unexpectedSyntaxError
                     , exception.SourcePosn - sourcePosn
                    );
            }
        }
    }
}