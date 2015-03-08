using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IOperator<TTreeItem>
    {
        TTreeItem Terminal(Token token);
        TTreeItem Prefix(Token token, TTreeItem right);
        TTreeItem Suffix(TTreeItem left, Token token);
        TTreeItem Infix(TTreeItem left, Token token, TTreeItem right);
    }
}