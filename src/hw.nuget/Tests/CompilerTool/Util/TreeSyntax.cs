using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TreeSyntax : Syntax
    {
        protected TreeSyntax(Syntax left, IToken token, Syntax right)
            : base(token)
        {
            Left = left;
            Right = right;
        }

        public sealed override Syntax Left { get; }
        public sealed override Syntax Right { get; }

        public override IEnumerable<IToken> Tokens
        {
            get
            {
                var result = Enumerable.Empty<IToken>();

                if(Left != null)
                    result = result.Concat(Left.Tokens);
                result = result.Concat(base.Tokens);
                if(Right != null)
                    result = result.Concat(Right.Tokens);
                return result;
            }
        }
    }
}