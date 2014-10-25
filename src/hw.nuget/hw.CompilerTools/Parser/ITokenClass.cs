using System;
using System.Collections.Generic;
using System.Linq;
using hw.PrioParser;

namespace hw.Parser
{
    interface INameProvider
    {
        string Name { set; }
    }

    interface ITokenClass : IType<ParsedSyntax>, INameProvider
    {}
}