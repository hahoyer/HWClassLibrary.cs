using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    public sealed class Token : DumpableObject, IToken
    {
        internal static Token CreateStart(Source source)
            => new Token(null, (source + 0).Span(0), true);

        internal static Token Create(ScannerToken token, BracketContext context)
            => new Token
                (
                token.PrecededWith,
                token.Characters,
                context.IsBracketAndLeftBracket(token.Id)
                );

        static int _nextObjectId;
        readonly WhiteSpaceToken[] _precededWith;
        readonly SourcePart _characters;
        readonly bool? IsBracketAndLeftBracket;

        Token
            (WhiteSpaceToken[] precededWith, SourcePart characters, bool? isBracketAndLeftBracket)
            : base(_nextObjectId++)
        {
            _precededWith = precededWith ?? new WhiteSpaceToken[0];
            _characters = characters;
            IsBracketAndLeftBracket = isBracketAndLeftBracket;
            StopByObjectIds();
        }

        [DisableDump]
        public SourcePart SourcePart => _characters + _precededWith.SourcePart();

        public string Id => _characters.Id;
        WhiteSpaceToken[] IToken.PrecededWith => _precededWith;
        SourcePart IToken.Characters => _characters;

        bool? IToken.IsBracketAndLeftBracket => IsBracketAndLeftBracket;

        [UsedImplicitly]
        public new string NodeDump => Id;
    }
}