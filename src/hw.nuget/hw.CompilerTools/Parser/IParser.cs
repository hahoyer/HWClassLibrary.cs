using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IParser<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        TTreeItem Execute(SourcePosn start, Stack<OpenItem<TTreeItem>> stack = null);
        bool Trace{ get; set; }
    }

    public interface ISubParser<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        IType<TTreeItem> Execute(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack = null);
    }
}