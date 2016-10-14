using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class ParenthesisSyntax : TreeSyntax
    {
        internal readonly IToken Leftpart;

        public ParenthesisSyntax(IToken leftpart, Syntax middle, IToken rightPart)
            : base(middle, rightPart, null) { Leftpart = leftpart; }

        public override string TokenClassName => IsValid ? "()" : "?(?";

        public override IEnumerable<IToken> Tokens => new[] {Leftpart}.Concat(base.Tokens);

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }

        internal bool IsValid
        {
            get
            {
                var left = Leftpart.Characters.Id;
                if(left == "")
                    left = PrioTable.BeginOfText;

                var right = Token.Characters.Id;
                if(right == "")
                    right = PrioTable.EndOfText;

                var index = MainTokenFactory.RightBrackets
                    .IndexWhere(item => { return item == right; })
                    .AssertValue();
                return MainTokenFactory.LeftBrackets[index] == left;
            }
        }
    }
}