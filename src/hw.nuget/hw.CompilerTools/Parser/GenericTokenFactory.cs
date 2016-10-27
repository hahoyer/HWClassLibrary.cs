using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class GenericTokenFactory<TTreeItem> : PredefinedTokenFactory<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        protected override IEnumerable<IParserTokenType<TTreeItem>> GetPredefinedTokenClasses()
            => PredefinedTokenClasses;

        protected GenericTokenFactory()
        {
            PredefinedTokenClasses = GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);
        }

        public readonly IEnumerable<IParserTokenType<TTreeItem>> PredefinedTokenClasses;

        IEnumerable<IParserTokenType<TTreeItem>> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<VariantAttribute>(true).ToArray();
            if (variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance<TTreeItem>(type));

            return new[] { SpecialTokenClass(type) };
        }

        protected virtual IParserTokenType<TTreeItem> SpecialTokenClass(System.Type type)
            => (IParserTokenType<TTreeItem>)Activator.CreateInstance(type);

        bool BelongsToFactory(System.Type type)
        {
            var thisType = GetType();
            return type.Is<ScannerTokenType>()
                && !type.IsAbstract
                && type
                    .GetAttributes<BelongsToAttribute>(true)
                    .Any(attr => thisType.Is(attr.TokenFactory));
        }
    }
}