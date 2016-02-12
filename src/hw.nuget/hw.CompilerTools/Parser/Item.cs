using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem> : DumpableObject, PrioTable.IItem
        where TTreeItem : class, ISourcePart
    {
        [EnableDump]
        internal readonly IType<TTreeItem> Type;
        internal readonly ScannerToken Token;
        internal readonly PrioTable.Context Context;

        internal Item(IType<TTreeItem> type, ScannerToken token, PrioTable.Context context)
        {
            Type = type;
            Token = token;
            Context = context;
        }

        [EnableDump]
        internal int Depth => Context?.Depth??0;

        internal Item(Scanner<TTreeItem>.Item other, PrioTable.Context context)
            : this(other.Type.Type, other.Token, context) {}

        internal TTreeItem Create(TTreeItem left, TTreeItem right)
        {
            return Type.Create(left, new Token(Token.PrecededWith, Token.Characters), right);
        }

        PrioTable.Context PrioTable.IItem.Context => Context;
        string PrioTable.IItem.Token => Token.Id;
    }
}