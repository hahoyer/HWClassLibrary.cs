using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IOperator<TTreeItem>
    {
        TTreeItem Terminal(SourcePart token);
        TTreeItem Prefix(SourcePart token, TTreeItem right);
        TTreeItem Suffix(TTreeItem left, SourcePart token);
        TTreeItem Infix(TTreeItem left, SourcePart token, TTreeItem right);
    }
}