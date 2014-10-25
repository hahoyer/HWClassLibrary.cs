using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IParser<TTreeItem>
        where TTreeItem : class
    {
        TTreeItem Execute
            (SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack);
    }

    public interface ISubParser<TTreeItem>
        where TTreeItem : class
    {
        IType<TTreeItem> Execute
            (SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack);
    }

}