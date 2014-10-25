using System;
using System.Collections.Generic;
using System.Linq;
using hw.PrioParser;

namespace hw.Parser
{
    interface ITokenFactory<TTreeItem, in TPart>
    {
        IType<TTreeItem, TPart> TokenClass(string name);
        PrioTable PrioTable { get; }
        IType<TTreeItem, TPart> Number { get; }
        IType<TTreeItem, TPart> Text { get; }
        IType<TTreeItem, TPart> EndOfText { get; }
    }
}