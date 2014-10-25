using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public interface IType<TTreeItem, in TPart> 
    {
        TTreeItem Create(TTreeItem left, TPart part, TTreeItem right, bool isMatch);
        string PrioTableName { get; }
        bool IsEnd { get; }
    }
}