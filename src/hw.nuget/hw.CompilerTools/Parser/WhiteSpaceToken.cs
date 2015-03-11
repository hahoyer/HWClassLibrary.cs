using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    public sealed class WhiteSpaceToken
    {
        public readonly int Index;
        public readonly SourcePart Characters;
        public WhiteSpaceToken(int index, SourcePart characters)
        {
            Index = index;
            Characters = characters;
        }
        [UsedImplicitly]
        public string NodeDump { get { return Characters.NodeDump + "." + Index + "i"; } }
    }
}