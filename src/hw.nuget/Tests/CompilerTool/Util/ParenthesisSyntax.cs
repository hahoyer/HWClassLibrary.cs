using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    [PublicAPI]
    sealed class ParenthesisSyntax : TreeSyntax
    {
        internal readonly IToken LeftPart;

        public ParenthesisSyntax(IToken leftPart, Syntax middle, IToken rightPart)
            : base(middle, rightPart, null)
            => LeftPart = leftPart;

        public override string TokenClassName => IsValid? "()" : "?(?";

        public override IEnumerable<IToken> Tokens => new[] {LeftPart}.Concat(base.Tokens);

        internal bool IsValid
        {
            get
            {
                var left = LeftPart.Characters.Id;
                if(left == "")
                    left = PrioTable.BeginOfText;

                var right = Token.Characters.Id;
                if(right == "")
                    right = PrioTable.EndOfText;

                var index = TokenFactory.RightBrackets
                    .IndexWhere(item => item == right)
                    .AssertValue();
                return TokenFactory.LeftBrackets[index] == left;
            }
        }

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}