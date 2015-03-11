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
    public sealed class ScannerItem<TTreeItem>
        where TTreeItem : class
    {
        public readonly IType<TTreeItem> Type;
        public readonly Token Token;

        public ScannerItem(IType<TTreeItem> type, Token token)
        {
            Type = type;
            Token = token;
        }
    }

    [DebuggerDisplay("{NodeDump}")]
    public sealed class Token
    {
        public readonly WhiteSpaceToken[] PrecededWith;
        public readonly SourcePart Characters;

        public Token(SourcePart characters, WhiteSpaceToken[] precededWith)
        {
            Characters = characters;
            PrecededWith = precededWith ?? new WhiteSpaceToken[0];
            AssertValid();
        }

        void AssertValid()
        {
            for(var i = 1; i < PrecededWith.Length; i++)
                Tracer.Assert
                    (PrecededWith[i - 1].Characters.End.Equals(PrecededWith[i].Characters.Start));
            var l = PrecededWith.LastOrDefault();
            if(l == null)
                return;
            Tracer.Assert(l.Characters.End.Equals(Characters.Start));
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get { return (Characters + PrecededWith.Select(item => item.Characters).Aggregate()); }
        }

        public string Name { get { return Characters.Name; } }

        [UsedImplicitly]
        public string NodeDump { get { return Name; } }
    }

    [DebuggerDisplay("{NodeDump}")]
    public sealed class WhiteSpaceToken
    {
        public readonly int Index;
        public readonly SourcePart Characters;
        public WhiteSpaceToken(int index, SourcePart characters)
        {
            Index = index;
            Characters = characters;
        }
        [UsedImplicitly]
        public string NodeDump { get { return Characters.NodeDump + "." + Index + "i"; } }
    }
}