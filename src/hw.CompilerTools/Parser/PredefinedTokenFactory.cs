using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public abstract class PredefinedTokenFactory<TParserResult> : ScannerTokenType<TParserResult>
    where TParserResult : class
{
    readonly ValueCache<FunctionCache<string, IParserTokenType<TParserResult>>> PredefinedTokenClassesCache;

    protected PredefinedTokenFactory() => PredefinedTokenClassesCache = new(GetDictionary);

    protected abstract IEnumerable<IParserTokenType<TParserResult>> GetPredefinedTokenClasses();
    protected abstract IParserTokenType<TParserResult> GetTokenClass(string name);

    /// <summary>
    ///     Override this method, when the dictionary requires a key different from occurrence found in source,
    ///     for instance, when your language is not case-sensitive or for names only some first characters are significant.
    ///     To register the names actually used, <see cref="IAliasKeeper" />.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Default implementation returns the id.</returns>
    protected virtual string GetTokenClassKeyFromToken(string id) => id;

    [PublicAPI]
    protected sealed override IParserTokenType<TParserResult> GetParserTokenType(string id)
    {
        var key = GetTokenClassKeyFromToken(id);
        var result = PredefinedTokenClassesCache.Value[key];
        // ReSharper disable SuspiciousTypeConversion.Global
        (result as IAliasKeeper)?.Add(id);
        return result;
    }

    FunctionCache<string, IParserTokenType<TParserResult>> GetDictionary()
        => new
        (
            GetPredefinedTokenClasses().ToDictionary(item => GetTokenClassKeyFromToken(item.PrioTableId)),
            GetTokenClass
        );
}

/// <summary>
///     Use this interface at your <see cref="IParserTokenType&lt;TParserResult&gt;" /> to register names that are actually
///     used for your token type.
/// </summary>
[PublicAPI]
public interface IAliasKeeper
{
    /// <summary>
    ///     Method is called for every occurrence
    /// </summary>
    /// <param name="id">the actual name version</param>
    void Add(string id);
}