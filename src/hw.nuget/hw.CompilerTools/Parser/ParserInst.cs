using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    sealed class Position : Dumpable, IPosition<IParsedSyntax, TokenData>
    {
        internal readonly SourcePosn SourcePosn;
        readonly Scanner _scanner;
        readonly ITokenFactory<IParsedSyntax, TokenData> _tokenFactory;

        Position(SourcePosn sourcePosn, ITokenFactory<IParsedSyntax, TokenData> tokenFactory, Scanner scanner)
        {
            SourcePosn = sourcePosn;
            _tokenFactory = tokenFactory;
            _scanner = scanner;
        }

        Item<IParsedSyntax, TokenData> IPosition<IParsedSyntax, TokenData>.GetItemAndAdvance(Stack<OpenItem<IParsedSyntax, TokenData>> stack)
        {
            return _scanner.CreateToken(SourcePosn, _tokenFactory, stack);
        }

        TokenData IPosition<IParsedSyntax, TokenData>.Span(IPosition<IParsedSyntax, TokenData> end) { return TokenData.Span(SourcePosn, end); }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        public static IParsedSyntax Parse
            (Source source, ITokenFactory<IParsedSyntax, TokenData> tokenFactory, Scanner scanner, Stack<OpenItem<IParsedSyntax, TokenData>> stack = null)
        {
            return Parse(source + 0, tokenFactory, scanner, stack);
        }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        public static IParsedSyntax Parse
            (SourcePosn sourcePosn, ITokenFactory<IParsedSyntax, TokenData> tokenFactory, Scanner scanner, Stack<OpenItem<IParsedSyntax, TokenData>> stack = null)
        {
            var p = new Position(sourcePosn, tokenFactory, scanner);
            return p.Parse(tokenFactory.PrioTable, stack);
        }
    }
}