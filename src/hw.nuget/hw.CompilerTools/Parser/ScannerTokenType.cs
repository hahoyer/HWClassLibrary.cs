using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Connects the scanner and the parser by providing an implementation of GetTokenType
/// </summary>
public abstract class ScannerTokenType
    : DumpableObject
        , IScannerTokenType
        , IParserTokenFactory
{
    IParserTokenType<TSourcePart> IParserTokenFactory.GetTokenType<TSourcePart>(string id)
        => GetParserTokenType<TSourcePart>(id);

    string IScannerTokenType.Id => GetType().PrettyName();

    IParserTokenFactory IScannerTokenType.ParserTokenFactory => this;

    /// <summary>
    ///     Helper function to map from generic method to generic class
    /// </summary>
    /// <typeparam name="TSourcePart"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    protected abstract IParserTokenType<TSourcePart> GetParserTokenType<TSourcePart>(string id)
        where TSourcePart : class;
}

/// <summary>
///     Generic variant of <see cref="ScannerTokenType" />
/// </summary>
/// <typeparam name="TSourcePart"></typeparam>
public abstract class ScannerTokenType<TSourcePart> : ScannerTokenType
    where TSourcePart : class
{
    /// <summary>
    ///     Create the parser token type from characters found by the scanner (stripped by whitespaces).
    /// </summary>
    /// <param name="id">characters without whitespaces</param>
    /// <returns></returns>
    protected abstract IParserTokenType<TSourcePart> GetParserTokenType(string id);

    protected sealed override IParserTokenType<TSourcePartTarget> GetParserTokenType<TSourcePartTarget>(string id)
    {
        (typeof(TSourcePart) == typeof(TSourcePartTarget)).Assert();
        return (IParserTokenType<TSourcePartTarget>)GetParserTokenType(id);
    }
}