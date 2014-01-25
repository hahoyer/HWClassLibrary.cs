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