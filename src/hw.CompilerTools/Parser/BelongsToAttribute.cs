

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Use this for token types to associate them with a token factory or any derivatives of this factory.
///     The token factory must be a <see cref="GenericTokenFactory{TSourcePart}" />
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[MeansImplicitUse]
[PublicAPI]
public sealed class BelongsToAttribute(Type tokenFactory) : Attribute
{
    public Type TokenFactory { get; } = tokenFactory;
}