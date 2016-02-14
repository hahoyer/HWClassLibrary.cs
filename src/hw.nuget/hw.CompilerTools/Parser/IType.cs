using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IType<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);
        string PrioTableId { get; }
    }

    public interface IBracketMatch<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        IType<TTreeItem> Value { get; }
    }
}