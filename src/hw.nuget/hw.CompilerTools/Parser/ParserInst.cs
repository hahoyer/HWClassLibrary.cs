#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    ///     The parser singleton
    /// </summary>
    sealed class ParserInst
    {
        readonly Scanner _scanner;
        readonly ITokenFactory _tokenFactory;

        public ParserInst(Scanner scanner, ITokenFactory tokenFactory)
        {
            _scanner = scanner;
            _tokenFactory = tokenFactory;
        }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        /// <param name="source"> </param>
        /// <returns> </returns>
        public IParsedSyntax Compile(Source source)
        {
            IPosition<IParsedSyntax> sourcePosn = new Position(source, this);
            return sourcePosn.Parse(_tokenFactory.PrioTable);
        }

        [Obsolete("", true)]
        public IParsedSyntax OldCompile(Source source)
        {
            var sourcePosn = source + 0;
            var stack = new Stack<PushedSyntax>();
            stack.Push(new PushedSyntax(sourcePosn, _tokenFactory));
            while(true)
            {
                var token = _scanner.CreateToken(sourcePosn, stack.Peek().TokenFactory);
                IParsedSyntax result = null;
                do
                {
                    var relation = stack.Peek().Relation(token.Name);
                    if(relation != '+')
                        result = stack.Pop().Syntax(result);

                    if(relation != '-')
                    {
                        if(token.Type == _tokenFactory.EndOfText)
                            return token.Create(null, result);
                        stack.Push(new PushedSyntax(result, token, stack.Peek().TokenFactory));
                        result = null;
                    }
                } while(result != null);
            }
        }

        internal Item<IParsedSyntax> GetItemAndAdvance(SourcePosn sourcePosn) { return _scanner.CreateToken(sourcePosn, _tokenFactory); }
    }

    sealed class Position : Dumpable, IPosition<IParsedSyntax>
    {
        internal readonly SourcePosn SourcePosn;
        readonly ParserInst _parserInst;

        public Position(Source source, ParserInst parserInst)
        {
            SourcePosn = source + 0;
            _parserInst = parserInst;
        }

        Item<IParsedSyntax> IPosition<IParsedSyntax>.GetItemAndAdvance(Stack<OpenItem<IParsedSyntax>> stack) { return _parserInst.GetItemAndAdvance(SourcePosn); }
        IPart<IParsedSyntax> IPosition<IParsedSyntax>.Span(IPosition<IParsedSyntax> end) { return TokenData.Span(SourcePosn, end); }
    }
}