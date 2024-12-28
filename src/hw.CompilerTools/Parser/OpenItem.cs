﻿using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public sealed class OpenItem<TTreeItem> : DumpableObject
    where TTreeItem : class
{
    public readonly PrioTable.ITargetItem BracketItem;
    public readonly TTreeItem Left;
    public readonly IToken Token;
    public readonly IParserTokenType<TTreeItem> Type;

    internal OpenItem(TTreeItem left, Item<TTreeItem> current)
    {
        Left = left;
        Type = current.Type;
        Token = current;
        BracketItem = current;
    }

    protected override string GetNodeDump()
        => Tracer.Dump(Left) + " " + Type.GetType().PrettyName();

    internal int NextDepth => BracketContext.GetRightDepth(BracketItem);

    internal TTreeItem Create(TTreeItem right)
    {
        if(Type != null)
            return Type.Create(Left, Token, right);
        (Left == null).Assert();
        return right;
    }
}