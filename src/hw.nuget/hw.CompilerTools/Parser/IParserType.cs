using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IParserType<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);
        string PrioTableId { get; }
    }

    public interface IBracketMatch<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        IParserType<TTreeItem> Value { get; }
    }

    interface IParserTypeProvider
    {
        IParserType<TTreeItem> GetType<TTreeItem>(string id)
            where TTreeItem : class, ISourcePart;
    }
}