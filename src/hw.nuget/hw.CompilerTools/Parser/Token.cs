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
        static int _nextObjectId;
        readonly WhiteSpaceToken[] _precededWith;
        readonly SourcePart _characters;
        public Token
            (WhiteSpaceToken[] precededWith, SourcePart characters)
            : base(_nextObjectId++)
        {
            _precededWith = precededWith ?? new WhiteSpaceToken[0];
            _characters = characters;
        }

        [DisableDump]
        public SourcePart SourcePart { get { return _characters + _precededWith.SourcePart(); } }

        public string Id { get { return _characters.Id; } }
        WhiteSpaceToken[] IToken.PrecededWith { get { return _precededWith; } }
        SourcePart IToken.Characters { get { return _characters; } }

        [UsedImplicitly]
        public new string NodeDump { get { return Id; } }
    }
}