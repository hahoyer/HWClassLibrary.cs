using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface ITokenFactory<TTreeItem>
        where TTreeItem : class
    {
        IType<TTreeItem> TokenClass(string name);
        IType<TTreeItem> Number { get; }
        IType<TTreeItem> Text { get; }
        IType<TTreeItem> EndOfText { get; }
        IType<TTreeItem> Error(Match.IError error);
    }
}