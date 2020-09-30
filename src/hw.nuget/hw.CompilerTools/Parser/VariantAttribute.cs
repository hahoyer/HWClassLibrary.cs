using System;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.Parser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    [PublicAPI]
    public sealed class VariantAttribute : Attribute
    {
        public object[] CreationParameter { get; }

        public VariantAttribute(params object[] creationParameter) => CreationParameter = creationParameter;

        public IParserTokenType<TTreeItem> CreateInstance<TTreeItem>(Type type)
            where TTreeItem : class => (IParserTokenType<TTreeItem>)type
            .GetConstructor(CreationParameter.Select(p => p.GetType()).ToArray())
            .AssertNotNull()
            .Invoke(CreationParameter);
    }
}