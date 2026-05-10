using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Memoising dictionary that lazily computes missing values via a producer delegate. Behaves
///     like a regular <see cref="Dictionary{TKey,TValue}" /> for already-present entries; reading an
///     unknown key triggers the producer and stores its result.
/// </summary>
/// <typeparam name="TKey"> Lookup key type. </typeparam>
/// <typeparam name="TValue"> Cached value type. </typeparam>
[AdditionalNodeInfo("NodeDump")]
[PublicAPI]
public sealed class FunctionCache<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull
{
    sealed class NoCaseComparer : IEqualityComparer<string>
    {
        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        /// <returns> true if the specified objects are equal; otherwise, false. </returns>
        /// <param name="other"> The second object of type T to compare. </param>
        /// <param name="target"> The first object of type T to compare. </param>
        public bool Equals(string? target, string? other) => target?.ToUpperInvariant() == other?.ToUpperInvariant();

        /// <summary>
        ///     When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and
        ///     data structures, such as a hash table.
        /// </summary>
        /// <returns> A hash code for the specified object. </returns>
        /// <param name="obj"> The object for which to get a hash code. </param>
        /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(string obj) => EqualityComparer<string>.Default.GetHashCode(obj.ToUpperInvariant());
    }

    /// <summary>
    ///     Placeholder stored under the key for the duration of the producer call, so re-entrant
    ///     lookups during evaluation see this value instead of triggering recursion.
    /// </summary>
    public readonly TValue? DefaultValue;

    readonly Func<TKey, TValue> CreateValue;

    /// <summary>A shallow copy of the cache, sharing the producer but with independent storage.</summary>
    public FunctionCache<TKey, TValue> Clone => new(this);

    /// <summary>Title used by <see cref="AdditionalNodeInfoAttribute" /> to label this cache in tree dumps.</summary>
    [DisableDump]
    public string NodeDump => GetType().PrettyName();

    /// <summary>
    ///     Returns the value for <paramref name="key" />, computing and storing it via the producer
    ///     when not yet cached. Setting calls <see cref="Dictionary{TKey,TValue}.Add" />, so writing
    ///     to an existing key throws.
    /// </summary>
    public new TValue this[TKey key]
    {
        get
        {
            Ensure(key);
            return base[key]!;
        }
        set => Add(key, value);
    }

    /// <summary>Snapshot of currently cached keys as a freshly allocated array.</summary>
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

    /// <param name="createValue"> Producer invoked on first access of a key. </param>
    public FunctionCache(Func<TKey, TValue> createValue) => CreateValue = createValue;

    /// <param name="defaultValue"> See <see cref="DefaultValue" />. </param>
    /// <param name="createValue"> Producer invoked on first access of a key. </param>
    public FunctionCache(TValue defaultValue, Func<TKey, TValue> createValue)
    {
        DefaultValue = defaultValue;
        CreateValue = createValue;
    }

    /// <param name="comparer"> Equality comparer used for keys. </param>
    /// <param name="createValue"> Producer invoked on first access of a key. </param>
    public FunctionCache(IEqualityComparer<TKey> comparer, Func<TKey, TValue> createValue)
        : base(comparer)
        => CreateValue = createValue;

    /// <summary>Creates a copy of <paramref name="target" /> with a different key comparer.</summary>
    /// <param name="target"> Source cache; its producer is reused. </param>
    /// <param name="comparer"> Equality comparer for the copy. </param>
    public FunctionCache(FunctionCache<TKey, TValue> target, IEqualityComparer<TKey> comparer)
        : base(target, comparer)
        => CreateValue = target.CreateValue;

    /// <summary>Pre-fills the cache from <paramref name="target" /> and uses <paramref name="createValue" /> for unknown keys.</summary>
    /// <param name="target"> Initial entries. </param>
    /// <param name="createValue"> Producer invoked on first access of a key not in <paramref name="target" />. </param>
    public FunctionCache(IDictionary<TKey, TValue> target, Func<TKey, TValue> createValue)
        : base(target)
        => CreateValue = createValue;

    /// <summary>Copy constructor; the new cache shares the producer but has independent storage.</summary>
    /// <param name="target"> Cache whose entries and producer are copied. </param>
    public FunctionCache(FunctionCache<TKey, TValue> target)
        : base(target)
        => CreateValue = target.CreateValue;

    /// <summary>
    ///     Constructs a cache without a producer; reading an unknown key throws
    ///     <see cref="KeyNotFoundException" />. Useful when the cache is populated explicitly via
    ///     the indexer.
    /// </summary>
    public FunctionCache() => CreateValue = ThrowKeyNotFoundException;

    /// <summary>Whether <paramref name="key" /> is currently cached.</summary>
    public bool IsValid(TKey key) => ContainsKey(key);

    /// <summary>
    ///     Forces presence (<paramref name="value" /> = <c>true</c>) or absence
    ///     (<paramref name="value" /> = <c>false</c>) of the cache entry for <paramref name="key" />.
    /// </summary>
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
        if(!ReferenceEquals(DefaultValue, default(TValue)))
            base[key] = DefaultValue!;
        base[key] = CreateValue(key);
    }
}