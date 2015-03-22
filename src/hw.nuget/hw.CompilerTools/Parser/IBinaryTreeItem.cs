using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface IBinaryTreeItem
    {
        IBinaryTreeItem Right { get; }
        string TokenId { get; }
        IBinaryTreeItem Left { get; }
    }
}