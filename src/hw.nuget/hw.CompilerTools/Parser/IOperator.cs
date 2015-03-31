using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface IOperator<in TIn, out TOut>
    {
        TOut Terminal(IToken token);
        TOut Prefix(IToken token, TIn right);
        TOut Suffix(TIn left, IToken token);
        TOut Infix(TIn left, IToken token, TIn right);
    }
}