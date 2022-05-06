using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Provides a token factory that caches the items used.
///     It is also used internally, so you can define token factories,
///     that may use laborious functions in your token factory.
/// </summary>
/// <typeparam name="TTreeItem"></typeparam>
public sealed class CachingTokenFactory<TTreeItem>
    : Dumpable
        , ITokenFactory<TTreeItem>
    where TTreeItem : class
{
    readonly ValueCache<IParserTokenType<TTreeItem>> BeginOfTextCache;
    readonly ValueCache<LexerItem[]> ClassesCache;
    readonly ValueCache<IScannerTokenType> EndOfTextCache;
    readonly ValueCache<IScannerTokenType> InvalidCharacterErrorCache;

    public CachingTokenFactory(ITokenFactory<TTreeItem> target)
    {
        EndOfTextCache = new(() => target.EndOfText);
        BeginOfTextCache = new(() => target.BeginOfText);
        InvalidCharacterErrorCache = new(() => target.InvalidCharacterError);
        ClassesCache = new(() => target.Classes);
    }

    LexerItem[] ITokenFactory.Classes => ClassesCache.Value;
    IScannerTokenType ITokenFactory.EndOfText => EndOfTextCache.Value;
    IScannerTokenType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;

    IParserTokenType<TTreeItem> ITokenFactory<TTreeItem>.BeginOfText => BeginOfTextCache.Value;
}