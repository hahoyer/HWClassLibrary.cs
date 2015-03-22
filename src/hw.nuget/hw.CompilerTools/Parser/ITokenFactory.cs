using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface ITokenFactory<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        Scanner<TTreeItem>.IType TokenClass(string name);
        Scanner<TTreeItem>.IType Number { get; }
        Scanner<TTreeItem>.IType Text { get; }
        Scanner<TTreeItem>.IType EndOfText { get; }
        Scanner<TTreeItem>.IType Error(Match.IError error);
    }
}