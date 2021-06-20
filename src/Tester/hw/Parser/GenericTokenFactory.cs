using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [PublicAPI]
    public abstract class GenericTokenFactory<TSourcePart> : PredefinedTokenFactory<TSourcePart>
        where TSourcePart : class
    {
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

        protected override IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses()
            => PredefinedTokenClasses;

        protected virtual IParserTokenType<TSourcePart> SpecialTokenClass(Type type)
            => (IParserTokenType<TSourcePart>)Activator.CreateInstance(type);

        IEnumerable<IParserTokenType<TSourcePart>> CreateInstance(Type type)
        {
            var variants = type.GetAttributes<VariantAttribute>(true).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance<TSourcePart>(type));

            return new[] {SpecialTokenClass(type)};
        }

        bool BelongsToFactory(Type type)
        {
            var thisType = GetType();
            return type.Is<ScannerTokenType>() &&
                   !type.IsAbstract &&
                   type
                       .GetAttributes<BelongsToAttribute>(true)
                       .Any(attr => thisType.Is(attr.TokenFactory));
        }
    }
}