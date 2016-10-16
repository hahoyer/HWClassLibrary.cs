using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface IUniqueIdProvider
    {
        string Value { get; }
    }
}