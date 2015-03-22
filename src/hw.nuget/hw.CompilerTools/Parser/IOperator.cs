using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface IOperator<TTreeItem>
    {
        TTreeItem Terminal(IToken token);
        TTreeItem Prefix(IToken token, TTreeItem right);
        TTreeItem Suffix(TTreeItem left, IToken token);
        TTreeItem Infix(TTreeItem left, IToken token, TTreeItem right);
    }
}