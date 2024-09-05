using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Dictionary that does not allow null values
/// </summary>
/// <typeparam name="TKey"> </typeparam>
/// <typeparam name="TValue"> </typeparam>
[AdditionalNodeInfo("NodeDump")]
[PublicAPI]
public sealed class FunctionCache<TKey, TValue> : Dictionary<TKey, TValue>
{
    sealed class NoCaseComparer : IEqualityComparer<string>
    {
        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <returns> true if the specified objects are equal; otherwise, false. </returns>
        /// <param name="other"> The second object of type T to compare. </param>
        /// <param name="target"> The first object of type T to compare. </param>
        public bool Equals(string target, string other) => target?.ToUpperInvariant() == other?.ToUpperInvariant();

        /// <summary>
        ///     When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and
        ///     data structures, such as a hash table.
        /// </summary>
        /// <returns> A hash code for the specified object. </returns>
        /// <param name="obj"> The object for which to get a hash code. </param>
        /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(string obj) => EqualityComparer<string>.Default.GetHashCode(obj.ToUpperInvariant());
    }

    public readonly TValue DefaultValue;
    readonly Func<TKey, TValue> CreateValue;

    public FunctionCache(Func<TKey, TValue> createValue) => CreateValue = createValue;

    public FunctionCache(TValue defaultValue, Func<TKey, TValue> createValue)
    {
        DefaultValue = defaultValue;
        CreateValue = createValue;
    }

    public FunctionCache(IEqualityComparer<TKey> comparer, Func<TKey, TValue> createValue)
        : base(comparer)
        => CreateValue = createValue;

    public FunctionCache(FunctionCache<TKey, TValue> target, IEqualityComparer<TKey> comparer)
        : base(target, comparer)
        => CreateValue = target.CreateValue;

    public FunctionCache(IDictionary<TKey, TValue> target, Func<TKey, TValue> createValue)
        : base(target)
        => CreateValue = createValue;

    public FunctionCache(FunctionCache<TKey, TValue> target)
        : base(target)
        => CreateValue = target.CreateValue;

    public FunctionCache() => CreateValue = ThrowKeyNotFoundException;

    public FunctionCache<TKey, TValue> Clone => new(this);

    [DisableDump]
    public string NodeDump => GetType().PrettyName();

    /// <summary>
    ///     Gets the value with the specified key
    /// </summary>
    /// <value> </value>
    /// created 13.01.2007 15:43
    public new TValue this[TKey key]
    {
        get
        {
            Ensure(key);
            return base[key];
        }
        set => Add(key, value);
    }

    public new TKey[] Keys
    {
        get
        {
            var keys = base.Keys;
            var result = new TKey[keys.Count];
            var i = 0;
            foreach(var key in keys)
                result[i++] = key;
            return result;
        }
    }

    public bool IsValid(TKey key) => ContainsKey(key);

    public void IsValid(TKey key, bool value)
    {
        if(value)
            Ensure(key);
        else if(ContainsKey(key))
            Remove(key);
    }

    static TValue ThrowKeyNotFoundException(TKey key) => throw new KeyNotFoundException(key.ToString());

    void Ensure(TKey key)
    {
        if(ContainsKey(key))
            return;
        base[key] = DefaultValue;
        base[key] = CreateValue(key);
    }
}