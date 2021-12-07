using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    /// <summary>
    /// Complete token obtained by the scanner, including preceding whitespaces
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Whitespace items that precede this token.
        /// </summary>
        [DisableDump]
        IEnumerable<IItem> PrecededWith { get; }

        /// <summary>
        /// Characters the form the actual token.
        /// </summary>
        [DisableDump]
        SourcePart Characters { get; }

        /// <summary>
        /// Will be null if it is not bracket, true for left bracket and false for right bracket
        /// </summary>
        [DisableDump]
        bool? IsBracketAndLeftBracket { get; }

    }

    public static class TokenExtension
    {
        public static SourcePart SourcePart(this IToken token)
            => token.PrecededWith.SourcePart() + token.Characters;
    }
}