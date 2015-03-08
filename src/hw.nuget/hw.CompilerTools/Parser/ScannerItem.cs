using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Scanner;

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

    public sealed class Token
    {
        public readonly WhiteSpaceToken[] PreceededBy;
        public readonly SourcePart Characters;

        public Token(SourcePart characters, WhiteSpaceToken[] preceededBy)
        {
            Characters = characters;
            PreceededBy = preceededBy ?? new WhiteSpaceToken[0];
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get { return (Characters + PreceededBy.Select(item => item.Characters).Aggregate()); }
        }
        public string Name { get { return Characters.Name; } }
    }

    public sealed class WhiteSpaceToken
    {
        public readonly int Index;
        public readonly SourcePart Characters;
        public WhiteSpaceToken(int index, SourcePart characters)
        {
            Index = index;
            Characters = characters;
        }
    }
}