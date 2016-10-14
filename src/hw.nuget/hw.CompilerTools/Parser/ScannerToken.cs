using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    public sealed class ScannerToken
    {
        public static ScannerToken Create(IItem[] items)
            => new ScannerToken(items.Take(items.Length - 1), items.Last().SourcePart);

        public readonly IItem[] PrefixItems;
        public readonly SourcePart Characters;

        ScannerToken(IEnumerable<IItem> prefixItems, SourcePart characters)
        {
            PrefixItems = prefixItems?.ToArray() ?? new IItem[0];
            Characters = characters;
        }

        [UsedImplicitly]
        public string Id => Characters.Id;

        [UsedImplicitly]
        public string NodeDump => Id;
    }
}