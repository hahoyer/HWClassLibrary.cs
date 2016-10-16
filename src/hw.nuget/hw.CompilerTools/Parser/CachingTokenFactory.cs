using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.Parser
{
    sealed class CachingTokenFactory : Dumpable, ITokenFactory
    {
        readonly ITokenFactory Target;
        readonly ValueCache<IScannerTokenType> EndOfTextCache;
        readonly ValueCache<IScannerTokenType> InvalidCharacterErrorCache;
        readonly ValueCache<LexerItem[]> ClassesCache;

        internal CachingTokenFactory(ITokenFactory target)
        {
            Target = target;
            EndOfTextCache = new ValueCache<IScannerTokenType>(() => Target.EndOfText);
            InvalidCharacterErrorCache = new ValueCache<IScannerTokenType>(() => Target.InvalidCharacterError);
            ClassesCache = new ValueCache<LexerItem[]>(() => Target.Classes);
        }

        IScannerTokenType ITokenFactory.EndOfText => EndOfTextCache.Value;
        IScannerTokenType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;
        LexerItem[] ITokenFactory.Classes => ClassesCache.Value;
    }
}