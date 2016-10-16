using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Scanner
{
    public sealed class StringSourceProvider : ISourceProvider
    {
        readonly string Data;
        public StringSourceProvider(string data) { Data = data; }
        string ISourceProvider.Data => Data;
        bool ISourceProvider.IsPersistent => false;
    }
}