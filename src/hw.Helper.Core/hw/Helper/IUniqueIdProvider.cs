// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Produces a stable identifier for an object. Implementations are expected to return the same
///     <see cref="Value" /> for the lifetime of the instance.
/// </summary>
public interface IUniqueIdProvider
{
    /// <summary>The identifier.</summary>
    string Value { get; }
}