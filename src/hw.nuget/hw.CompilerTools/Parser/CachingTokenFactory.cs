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
        readonly ValueCache<IScannerType> EndOfTextCache;
        readonly ValueCache<IScannerType> InvalidCharacterErrorCache;
        readonly ValueCache<ILexerItem[]> ClassesCache;

        internal CachingTokenFactory(ITokenFactory target)
        {
            Target = target;
            EndOfTextCache = new ValueCache<IScannerType>(() => Target.EndOfText);
            InvalidCharacterErrorCache = new ValueCache<IScannerType>(() => Target.InvalidCharacterError);
            ClassesCache = new ValueCache<ILexerItem[]>(() => Target.Classes);
        }

        IScannerType ITokenFactory.EndOfText => EndOfTextCache.Value;
        IScannerType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;
        ILexerItem[] ITokenFactory.Classes => ClassesCache.Value;
    }
}