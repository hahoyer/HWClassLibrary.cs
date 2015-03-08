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
        public readonly SourcePart Part;

        public Token(SourcePart part, WhiteSpaceToken[] preceededBy)
        {
            Part = part;
            PreceededBy = preceededBy ?? new WhiteSpaceToken[0];
        }

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get { return (Part + PreceededBy.Select(item => item.Part).Aggregate()); }
        }
        public string Name { get { return Part.Name; } }
    }

    public sealed class WhiteSpaceToken
    {
        public readonly int Index;
        public readonly SourcePart Part;
        public WhiteSpaceToken(int index, SourcePart part)
        {
            Index = index;
            Part = part;
        }
    }
}