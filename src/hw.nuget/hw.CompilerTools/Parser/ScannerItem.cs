using System;
using System.Collections.Generic;
using System.Linq;

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

        public TTreeItem Create(TTreeItem left, TTreeItem right)
        {
            return Type.Create(left, Token, right);
        }
    }
}