using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IType<TTreeItem>
        where TTreeItem : class
    {
        TTreeItem Create(TTreeItem left, SourcePart part, TTreeItem right, bool isMatch);
        string PrioTableName { get; }
        ISubParser<TTreeItem> Next { get; }
    }
}