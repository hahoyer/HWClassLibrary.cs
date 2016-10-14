using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.Parser
{
    abstract class TokenFactory : Dumpable, ITokenFactory
    {
        readonly ValueCache<IScannerType> EndOfTextCache;
        readonly ValueCache<IScannerType> InvalidCharacterErrorCache;
        readonly ValueCache<ILexerItem[]> ClassesCache;

        internal TokenFactory()
        {
            EndOfTextCache = new ValueCache<IScannerType>(GetEndOfText);
            InvalidCharacterErrorCache = new ValueCache<IScannerType>(GetInvalidCharacterError);
            ClassesCache = new ValueCache<ILexerItem[]>(GetClasses);
        }

        IScannerType ITokenFactory.EndOfText => EndOfTextCache.Value;
        IScannerType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;
        ILexerItem[] ITokenFactory.Classes => ClassesCache.Value;

        protected abstract IScannerType GetInvalidCharacterError();
        protected abstract IScannerType GetEndOfText();
        protected abstract ILexerItem[] GetClasses();
    }
}