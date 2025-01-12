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
    IParserTokenType<TParserResult> IParserTokenFactory.GetTokenType<TParserResult>(string id)
        => GetParserTokenType<TParserResult>(id);

    string IScannerTokenType.Id => GetType().PrettyName();

    IParserTokenFactory IScannerTokenType.ParserTokenFactory => this;

    /// <summary>
    ///     Helper function to map from generic method to generic class
    /// </summary>
    /// <typeparam name="TParserResult"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    protected abstract IParserTokenType<TParserResult> GetParserTokenType<TParserResult>(string id)
        where TParserResult : class;
}

/// <summary>
///     Generic variant of <see cref="ScannerTokenType" />
/// </summary>
/// <typeparam name="TParserResult"></typeparam>
public abstract class ScannerTokenType<TParserResult> : ScannerTokenType
    where TParserResult : class
{
    /// <summary>
    ///     Create the parser token type from characters found by the scanner (stripped by whitespaces).
    /// </summary>
    /// <param name="id">characters without whitespaces</param>
    /// <returns></returns>
    protected abstract IParserTokenType<TParserResult> GetParserTokenType(string id);

    protected sealed override IParserTokenType<TSourcePartTarget> GetParserTokenType<TSourcePartTarget>(string id)
    {
        (typeof(TParserResult) == typeof(TSourcePartTarget)).Assert();
        return (IParserTokenType<TSourcePartTarget>)GetParserTokenType(id);
    }
}