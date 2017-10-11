using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    /// Provides a token factory that caches the items used. 
    /// It is also used internally, so you can define token factories, 
    /// that may use labourious functions in your token factory.
    /// </summary>
    /// <typeparam name="TTreeItem"></typeparam>
    public sealed class CachingTokenFactory<TTreeItem> : Dumpable, ITokenFactory<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        readonly ValueCache<IParserTokenType<TTreeItem>> BeginOfTextCache;
        readonly ValueCache<LexerItem[]> ClassesCache;
        readonly ValueCache<IScannerTokenType> EndOfTextCache;
        readonly ValueCache<IScannerTokenType> InvalidCharacterErrorCache;
        readonly ITokenFactory<TTreeItem> Target;

        public CachingTokenFactory(ITokenFactory<TTreeItem> target)
        {
            Target = target;
            EndOfTextCache = new ValueCache<IScannerTokenType>(() => Target.EndOfText);
            BeginOfTextCache = new ValueCache<IParserTokenType<TTreeItem>>(() => Target.BeginOfText);
            InvalidCharacterErrorCache = new ValueCache<IScannerTokenType>(() => Target.InvalidCharacterError);
            ClassesCache = new ValueCache<LexerItem[]>(() => Target.Classes);
        }

        IParserTokenType<TTreeItem> ITokenFactory<TTreeItem>.BeginOfText => BeginOfTextCache.Value;
        IScannerTokenType ITokenFactory.EndOfText => EndOfTextCache.Value;
        IScannerTokenType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;
        LexerItem[] ITokenFactory.Classes => ClassesCache.Value;
    }
}