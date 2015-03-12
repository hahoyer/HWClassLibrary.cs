using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IScanner<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        ScannerItem<TTreeItem> NextToken(SourcePosn sourcePosn);
    }
}