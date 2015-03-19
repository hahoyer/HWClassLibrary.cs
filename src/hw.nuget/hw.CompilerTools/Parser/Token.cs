using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    public sealed class Token<TTreeItem> : IToken
        where TTreeItem : ISourcePart
    {
        public readonly TTreeItem[] OtherParts;
        readonly WhiteSpaceToken[] _precededWith;
        readonly SourcePart _characters;
        public Token
            (WhiteSpaceToken[] precededWith, SourcePart characters, params TTreeItem[] otherParts)
        {
            OtherParts = otherParts;
            _precededWith = precededWith ?? new WhiteSpaceToken[0];
            _characters = characters;
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get
            {
                return _characters +
                    _precededWith.Select(item => item.Characters).Aggregate();
            }
        }

        public string Id { get { return _characters.Id; } }
        WhiteSpaceToken[] IToken.PrecededWith { get { return _precededWith; } }
        SourcePart IToken.Characters { get { return _characters; } }

        TTreeItemParts[] IToken.OtherParts<TTreeItemParts>()
        {
            return OtherParts.Cast<TTreeItemParts>().ToArray();
        }

        [UsedImplicitly]
        public string NodeDump { get { return Id; } }
    }
}