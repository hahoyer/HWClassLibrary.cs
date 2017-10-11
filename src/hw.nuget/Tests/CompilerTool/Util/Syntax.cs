using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class Syntax : DumpableObject, IBinaryTreeItem, ISourcePartProxy
    {
        [DisableDump]
        public readonly IToken Token;

        protected Syntax(IToken token) { Token = token; }

        IBinaryTreeItem IBinaryTreeItem.Left => Left;

        string IBinaryTreeItem.TokenId => TokenClassName;
        IBinaryTreeItem IBinaryTreeItem.Right => Right;
        SourcePart ISourcePartProxy.All => SourcePart;
        public abstract Syntax Left { get; }

        public SourcePart SourcePart => Token.SourcePart();
        public abstract string TokenClassName { get; }
        public abstract Syntax Right { get; }

        public virtual IEnumerable<IToken> Tokens
        {
            get { yield return Token; }
        }

        protected override string Dump(bool isRecursion) => this.TreeDump();

        public abstract Syntax RightParenthesis(string id, IToken token, Syntax right);
    }
}