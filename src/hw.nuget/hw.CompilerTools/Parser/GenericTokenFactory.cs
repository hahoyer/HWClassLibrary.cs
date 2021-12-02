using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser
{
    /// <summary>
    ///     This token factory contains all <see cref="ScannerTokenType" /> classes that are not abstract and are
    ///     flagged with <see cref="BelongsToFactory" /> attribute pointing to this factory.
    /// </summary>
    [PublicAPI]
    public abstract class GenericTokenFactory<TSourcePart> : PredefinedTokenFactory<TSourcePart>
        where TSourcePart : class
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
        public readonly IEnumerable<IParserTokenType<TSourcePart>> PredefinedTokenClasses;

        [EnableDump]
        readonly string Title;

        protected GenericTokenFactory(string title = null)
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
        protected virtual IParserTokenType<TSourcePart> SpecialTokenClass(Type type)
            => (IParserTokenType<TSourcePart>)Activator.CreateInstance(type);

        protected sealed override IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses()
            => PredefinedTokenClasses;

        IEnumerable<IParserTokenType<TSourcePart>> CreateInstance(Type type)
        {
            var variants = type.GetAttributes<VariantAttribute>(true).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance<TSourcePart>(type));

            return new[] { SpecialTokenClass(type) };
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
}