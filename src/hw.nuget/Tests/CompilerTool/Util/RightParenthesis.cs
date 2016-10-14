using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class RightParenthesis : TokenClass<Syntax>, IBracketMatch<Syntax>
    {
        public RightParenthesis(string id) { Id = id; }
        public override string Id { get; }

        sealed class Matched : TokenClass<Syntax>
        {
            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => right == null ? left : new NamelessSyntax(left, token, right);

            public override string Id => "()";
        }

        IParserType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(left != null)
                return left.RightParenthesis(Id, token, right);

            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}