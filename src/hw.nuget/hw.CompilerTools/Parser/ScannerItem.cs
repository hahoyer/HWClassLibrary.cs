using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class ScannerItem<TTreeItem>
        where TTreeItem : class
    {
        public readonly IType<TTreeItem> Type;
        public readonly SourcePart Part;

        public ScannerItem(IType<TTreeItem> type, SourcePart part)
        {
            Type = type;
            Part = part;
        }
        public ParserItem<TTreeItem> ToParserItem { get { return new ParserItem<TTreeItem>(Type, Part); } }
    }
}