using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     This token factory contains all <see cref="ScannerTokenType" /> classes that are not abstract and are
///     flagged with <see cref="BelongsToFactory" /> attribute pointing to this factory.
/// </summary>
[PublicAPI]
public abstract class GenericTokenFactory<TParserResult> : PredefinedTokenFactory<TParserResult>
    where TParserResult : class
{
    /// <summary>
    ///     Token classes in this assembly that are
    ///     <list type="bullet">
    ///         <item>
    ///             <description>derived from <see cref="ScannerTokenType" /></description>
    ///         </item>
    ///         <item>
    ///             <description>not abstract</description>
    ///         </item>
    ///         <item>
    ///             <description>flagged with <see cref="BelongsToFactory" /> attribute pointing to this factory</description>
    ///         </item>
    ///     </list>
    /// </summary>
    public readonly IEnumerable<IParserTokenType<TParserResult>> PredefinedTokenClasses;

    [EnableDump]
    readonly string? Title;

    protected GenericTokenFactory(string? title = null)
    {
        Title = title;
        PredefinedTokenClasses = GetType()
            .Assembly
            .GetTypes()
            .Where(BelongsToFactory)
            .SelectMany(CreateInstance);
    }

    /// <summary>
    ///     Override this to provide special instantiation of a token class, that cannot be accomplished by variant attribute.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    protected virtual IParserTokenType<TParserResult> SpecialTokenClass(Type type)
        => (IParserTokenType<TParserResult>)Activator.CreateInstance(type)!;

    protected sealed override IEnumerable<IParserTokenType<TParserResult>> GetPredefinedTokenClasses()
        => PredefinedTokenClasses;

    IEnumerable<IParserTokenType<TParserResult>> CreateInstance(Type type)
    {
        var variants = type.GetAttributes<VariantAttribute>(true).ToArray();
        if(variants.Any())
            return variants
                .Select(variant => variant.CreateInstance<TParserResult>(type));

        return [SpecialTokenClass(type)];
    }

    bool BelongsToFactory(Type type)
    {
        var thisType = GetType();
        return type.Is<ScannerTokenType>() &&
            !type.IsAbstract &&
            type
                .GetAttributes<BelongsToAttribute>(true)
                .Any(attribute => thisType.Is(attribute.TokenFactory));
    }
}