using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    public interface IUniqueIdProvider
    {
        string Value { get; }
    }
}