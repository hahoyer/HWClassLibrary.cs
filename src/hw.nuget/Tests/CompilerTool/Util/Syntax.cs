using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class Syntax : DumpableObject, IBinaryTreeItem,ISourcePart
    {
        [DisableDump]
        public readonly IToken Token;

        protected Syntax(IToken token)
            { Token = token; }

        IBinaryTreeItem IBinaryTreeItem.Left => Left;
        public abstract Syntax Left { get; }

        string IBinaryTreeItem.TokenId => TokenClassName;
        SourcePart ISourcePart.All => SourcePart;

        public SourcePart SourcePart => Token.SourcePart();
        public abstract string TokenClassName { get; }
        public virtual bool TokenClassIsMain => true;
        IBinaryTreeItem IBinaryTreeItem.Right => Right;
        public abstract Syntax Right { get; }

        public virtual IEnumerable<IToken> Tokens { get { yield return Token; } }

        protected override string Dump(bool isRecursion) => this.TreeDump();

        public abstract Syntax RightParenthesis(string id, IToken token, Syntax right);

    }
}