using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IType<TTreeItem>
        where TTreeItem : class
    {
        TTreeItem Create(TTreeItem left, Token part, TTreeItem right);
        string PrioTableName { get; }
        ISubParser<TTreeItem> NextParser { get; }
        IType<TTreeItem> NextTypeIfMatched { get; }
    }
}