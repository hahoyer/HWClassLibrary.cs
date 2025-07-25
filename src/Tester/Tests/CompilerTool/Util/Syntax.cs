using hw.Parser;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util;

abstract class Syntax
    : DumpableObject
        , IBinaryTreeItem
{
    [DisableDump]
    public readonly IToken Token;

    protected Syntax(IToken token) => Token = token;

    IBinaryTreeItem? IBinaryTreeItem.Left => Left;
    IBinaryTreeItem? IBinaryTreeItem.Right => Right;

    string IBinaryTreeItem.TokenId => TokenClassName;
    public abstract Syntax? Left { get; }
    public abstract string TokenClassName { get; }
    public abstract Syntax? Right { get; }

    public abstract Syntax RightParenthesis(string id, IToken token, Syntax? right);

    public virtual IEnumerable<IToken> Tokens
    {
        get { yield return Token; }
    }

    protected override string Dump(bool isRecursion) => this.TreeDump();

    public SourcePart SourcePart => Token.GetSourcePart();
}