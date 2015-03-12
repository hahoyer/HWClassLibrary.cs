using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class ScannerItem<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        internal readonly IType<TTreeItem> Type;
        internal readonly ScannerToken Token;

        internal ScannerItem(IType<TTreeItem> type, ScannerToken token)
        {
            Type = type;
            Token = token;
        }

        internal TTreeItem Create(TTreeItem left, TTreeItem right)
        {
            return Type.Create(left, new Token<TTreeItem>(Token.PrecededWith, Token.Characters, left, right), right);
        }
    }
}