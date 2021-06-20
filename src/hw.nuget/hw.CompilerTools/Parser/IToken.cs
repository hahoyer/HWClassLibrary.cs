using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    public interface IToken
    {
        [DisableDump]
        IEnumerable<IItem> PrecededWith { get; }

        [DisableDump]
        SourcePart Characters { get; }

        [DisableDump]
        bool? IsBracketAndLeftBracket { get; }
    }

    public static class TokenExtension
    {
        public static SourcePart SourcePart(this IToken token)
            => token.PrecededWith.SourcePart() + token.Characters;
    }
}