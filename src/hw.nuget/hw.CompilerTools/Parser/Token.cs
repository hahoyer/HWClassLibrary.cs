using System;
using System.Collections.Generic;
using System.Diagnostics;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    sealed class ScannerToken
    {
        public readonly WhiteSpaceToken[] PrecededWith;
        public readonly SourcePart Characters;

        public ScannerToken(SourcePart characters, WhiteSpaceToken[] precededWith)
        {
            Characters = characters;
            PrecededWith = precededWith ?? new WhiteSpaceToken[0];
            AssertValid();
        }

        void AssertValid()
        {
            for (var i = 1; i < PrecededWith.Length; i++)
                Tracer.Assert
                    (PrecededWith[i - 1].Characters.End.Equals(PrecededWith[i].Characters.Start));
            var l = PrecededWith.LastOrDefault();
            if (l == null)
                return;
            Tracer.Assert(l.Characters.End.Equals(Characters.Start));
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get { return (Characters + PrecededWith.Select(item => item.Characters).Aggregate()); }
        }

        public string Id { get { return Characters.Id; } }

        [UsedImplicitly]
        public string NodeDump { get { return Id; } }
    }


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
            _precededWith = precededWith;
            _characters = characters;
            AssertValid();
        }

        static void AssertValid()
        {
            //ToDo
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get
            {
                return _characters +
                    _precededWith.Select(item => item.Characters).Aggregate() +
                    OtherParts.Where(item => item != null).Select(item => item.All).Aggregate();
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

    public interface IToken
    {
        SourcePosn Start { get; }
        SourcePart SourcePart { get; }
        string Id { get; }
        WhiteSpaceToken[] PrecededWith { get; }
        SourcePart Characters { get; }
        TTreeItem[] OtherParts<TTreeItem>();
    }
}