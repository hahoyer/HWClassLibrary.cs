using System;
using System.Collections.Generic;
using System.Linq;
using hw.PrioParser;

namespace hw.Parser
{
    interface ITokenFactory
    {
        IType TokenClass(string name);
        PrioTable PrioTable { get; }
        IType Number { get; }
        IType Text { get; }
        IType EndOfText { get; }
    }
}