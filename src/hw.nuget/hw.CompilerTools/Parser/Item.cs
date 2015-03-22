using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        internal readonly IType<TTreeItem> Type;
        internal readonly ScannerToken Token;

        internal Item(IType<TTreeItem> type, ScannerToken token)
        {
            Type = type;
            Token = token;
        }

        internal Item(Scanner<TTreeItem>.Item other)
            : this(other.Type.Type, other.Token) {}

        internal TTreeItem Create(TTreeItem left, TTreeItem right)
        {
            return Type.Create(left, new Token(Token.PrecededWith, Token.Characters), right);
        }
    }
}