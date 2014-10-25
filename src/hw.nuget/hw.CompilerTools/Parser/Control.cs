using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.Parser
{
    public sealed class Control<TTreeItem> : DumpableObject
        where TTreeItem : class
    {
        [Obsolete("", true)]
        public IScanner<TTreeItem> Scanner;
        [Obsolete("", true)]
        public ITokenFactory<TTreeItem> TokenFactory;
    }
}