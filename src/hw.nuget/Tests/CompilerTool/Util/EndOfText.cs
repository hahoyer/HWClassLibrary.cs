﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class EndOfText : CommonTokenType<Syntax>, IBracketMatch<Syntax>
    {
        protected override string Id => PrioTable.EndOfText;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(left != null)
                return left.RightParenthesis(Id, token, right);

            NotImplementedMethod(left, token, right);
            return null;
        }

        sealed class Matched : TokenClass<Syntax>
        {
            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                if(right == null)
                {
                    var result = (ParenthesisSyntax) left;
                    if(result.Right == null)
                        return result.Left;

                    NotImplementedMethod(left, token, right);
                    return null;
                }

                NotImplementedMethod(left, token, right);
                return null;
            }

            public override string Id => "<source>";
        }

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}