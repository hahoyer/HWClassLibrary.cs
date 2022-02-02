// ReSharper disable CheckNamespace

namespace hw.Parser;

public interface IBinaryTreeItem
{
    IBinaryTreeItem Right { get; }
    string TokenId { get; }
    IBinaryTreeItem Left { get; }
}