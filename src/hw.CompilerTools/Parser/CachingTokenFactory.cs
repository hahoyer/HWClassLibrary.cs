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
public sealed class CachingTokenFactory<TTreeItem>(ITokenFactory<TTreeItem> target)
    : Dumpable
        , ITokenFactory<TTreeItem>
    where TTreeItem : class
{
    readonly ValueCache<IParserTokenType<TTreeItem>?> BeginOfTextCache = new(() => target.BeginOfText);
    readonly ValueCache<LexerItem[]> ClassesCache = new(() => target.Classes);
    readonly ValueCache<IScannerTokenType> EndOfTextCache = new(() => target.EndOfText);
    readonly ValueCache<IScannerTokenType> InvalidCharacterErrorCache = new(() => target.InvalidCharacterError);

    LexerItem[] ITokenFactory.Classes => ClassesCache.Value;
    IScannerTokenType ITokenFactory.EndOfText => EndOfTextCache.Value;
    IScannerTokenType ITokenFactory.InvalidCharacterError => InvalidCharacterErrorCache.Value;

    IParserTokenType<TTreeItem>? ITokenFactory<TTreeItem>.BeginOfText => BeginOfTextCache.Value;
}