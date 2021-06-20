using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    abstract class Syntax
        : DumpableObject
            , IBinaryTreeItem
    {
        [DisableDump]
        public readonly IToken Token;

        protected Syntax(IToken token) => Token = token;
        public abstract Syntax Left { get; }

        public SourcePart SourcePart => Token.SourcePart();
        public abstract string TokenClassName { get; }
        public abstract Syntax Right { get; }

        public virtual IEnumerable<IToken> Tokens
        {
            get { yield return Token; }
        }

        IBinaryTreeItem IBinaryTreeItem.Left => Left;
        IBinaryTreeItem IBinaryTreeItem.Right => Right;

        string IBinaryTreeItem.TokenId => TokenClassName;

        public abstract Syntax RightParenthesis(string id, IToken token, Syntax right);

        protected override string Dump(bool isRecursion) => this.TreeDump();
    }
}