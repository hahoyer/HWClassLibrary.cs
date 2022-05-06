using System;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Use this for classes with <see cref="BelongsToAttribute" /> to define variants of token types
///     For each variant there must be a constructor that takes the arguments of the variant attribute as parameters.
/// </summary>
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