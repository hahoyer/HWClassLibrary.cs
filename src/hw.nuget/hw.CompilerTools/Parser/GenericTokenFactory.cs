using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class GenericTokenFactory<TSourcePart> : PredefinedTokenFactory<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        [EnableDump]
        readonly string Title;
        public readonly IEnumerable<IParserTokenType<TSourcePart>> PredefinedTokenClasses;

        protected GenericTokenFactory(string title=null)
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

        IEnumerable<IParserTokenType<TSourcePart>> CreateInstance(Type type)
        {
            var variants = type.GetAttributes<VariantAttribute>(inherit: true).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance<TSourcePart>(type));

            return new[] {SpecialTokenClass(type)};
        }

        protected virtual IParserTokenType<TSourcePart> SpecialTokenClass(Type type)
            => (IParserTokenType<TSourcePart>) Activator.CreateInstance(type);

        bool BelongsToFactory(Type type)
        {
            var thisType = GetType();
            return type.Is<ScannerTokenType>() &&
                   !type.IsAbstract &&
                   type
                       .GetAttributes<BelongsToAttribute>(inherit: true)
                       .Any(attr => thisType.Is(attr.TokenFactory));
        }
    }
}