using System;
using hw.DebugFormatter;

namespace hw.Scanner
{
    /// <summary>
    ///     Pair of a token class and a match function
    /// </summary>
    public sealed class LexerItem : DumpableObject
    {
        /// <summary>
        ///     The match function will get the current source position
        ///     and should return the number of characters that are accepted
        ///     or null if there is no match.
        /// </summary>
        public readonly Func<SourcePosition, int?> Match;

        public readonly IScannerTokenType ScannerTokenType;

        public LexerItem(IScannerTokenType scannerTokenType, Func<SourcePosition, int?> match)
        {
            ScannerTokenType = scannerTokenType;
            Match = match;
        }
    }
}