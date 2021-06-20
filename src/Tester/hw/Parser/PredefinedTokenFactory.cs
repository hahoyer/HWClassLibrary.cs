using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [PublicAPI]
    public abstract class PredefinedTokenFactory<TSourcePart> : ScannerTokenType<TSourcePart>
        where TSourcePart : class
    {
        readonly ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>
            PredefinedTokenClassesCache;

        protected PredefinedTokenFactory() => PredefinedTokenClassesCache =
            new ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>(GetDictionary);

        protected sealed override IParserTokenType<TSourcePart> GetParserTokenType(string id)
        {
            var key = GetTokenClassKeyFromToken(id);
            var result = PredefinedTokenClassesCache.Value[key];
            (result as IAliasKeeper)?.Add(id);
            return result;
        }

        /// <summary>
        ///     Override this method, when the dictionary requires a key different from occurrence found in source,
        ///     for instance, when your language is not case sensitive or for names only some first characters are significant.
        ///     To register the names actually used, <see cref="IAliasKeeper" />.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Default implementation returns the id.</returns>
        protected virtual string GetTokenClassKeyFromToken(string id) => id;

        protected abstract IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses();
        protected abstract IParserTokenType<TSourcePart> GetTokenClass(string name);

        FunctionCache<string, IParserTokenType<TSourcePart>> GetDictionary()
            => new FunctionCache<string, IParserTokenType<TSourcePart>>
            (
                GetPredefinedTokenClasses().ToDictionary(item => GetTokenClassKeyFromToken(item.PrioTableId)),
                GetTokenClass
            );
    }

    /// <summary>
    ///     Use this interface at your <see cref="IParserTokenType&lt;TSourcePart&gt;" /> to register names that are actually
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
}