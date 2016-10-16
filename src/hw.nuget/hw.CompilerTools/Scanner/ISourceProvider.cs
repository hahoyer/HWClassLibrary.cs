using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Scanner
{
    public interface ISourceProvider
    {
        string Data { get; }
        bool IsPersistent { get; }
    }
}