using System;
using System.Collections.Generic;
using System.Linq;
using hw.PrioParser;

namespace hw.Parser
{
    public interface ITokenFactory<TTreeItem>
    {
        IType<TTreeItem> TokenClass(string name);
        IType<TTreeItem> Number { get; }
        IType<TTreeItem> Text { get; }
        IType<TTreeItem> EndOfText { get; }
    }
}