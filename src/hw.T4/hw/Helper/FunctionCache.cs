using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Helper
{
    /// <summary>
    ///     Dicionary that does not allow null values
    /// </summary>
    /// <typeparam name="TKey"> </typeparam>
    /// <typeparam name="TValue"> </typeparam>
    [AdditionalNodeInfo("NodeDump")]
    public sealed class FunctionCache<TKey, TValue> : Dictionary<TKey, TValue>
    {
        readonly Func<TKey, TValue> _createValue;

        public FunctionCache(Func<TKey, TValue> createValue) { _createValue = createValue; }

        public FunctionCache(TValue defaultValue, Func<TKey, TValue> createValue)
        {
            DefaultValue = defaultValue;
            _createValue = createValue;
        }

        public FunctionCache(IEqualityComparer<TKey> comparer, Func<TKey, TValue> createValue)
            : base(comparer)
        {
            _createValue = createValue;
        }

        public FunctionCache(FunctionCache<TKey, TValue> x, IEqualityComparer<TKey> comparer)
            : base(x, comparer)
        {
            _createValue = x._createValue;
        }

        public FunctionCache(IDictionary<TKey, TValue> x, Func<TKey, TValue> createValue)
            : base(x)
        {
            _createValue = createValue;
        }

        public FunctionCache(FunctionCache<TKey, TValue> x)
            : base(x)
        {
            _createValue = x._createValue;
        }

        public FunctionCache() { _createValue = ThrowKeyNotFoundException; }

        static TValue ThrowKeyNotFoundException(TKey key)
        {
            throw new KeyNotFoundException(key.ToString());
        }

        public FunctionCache<TKey, TValue> Clone
        {
            get { return new FunctionCache<TKey, TValue>(this); }
        }

        [DisableDump]
        public string NodeDump { get { return GetType().PrettyName(); } }

        void Ensure(TKey key)
        {
            if(ContainsKey(key))
                return;
            base[key] = DefaultValue;
            base[key] = _createValue(key);
        }

        public readonly TValue DefaultValue;

        public bool IsValid(TKey key) { return ContainsKey(key); }

        public void IsValid(TKey key, bool value)
        {
            if(value)
                Ensure(key);
            else if(ContainsKey(key))
                Remove(key);
        }

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
            set { Add(key, value); }
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

        sealed class NoCaseComparer : IEqualityComparer<string>
        {
            static IEqualityComparer<string> _default;

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            /// <returns> true if the specified objects are equal; otherwise, false. </returns>
            /// <param name="y"> The second object of type T to compare. </param>
            /// <param name="x"> The first object of type T to compare. </param>
            public bool Equals(string x, string y)
            {
                return x.ToUpperInvariant() == y.ToUpperInvariant();
            }

            /// <summary>
            ///     When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and
            ///     data structures, such as a hash table.
            /// </summary>
            /// <returns> A hash code for the specified object. </returns>
            /// <param name="obj"> The object for which to get a hash code. </param>
            /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
            public int GetHashCode(string obj)
            {
                return EqualityComparer<string>.Default.GetHashCode(obj.ToUpperInvariant());
            }

            public static IEqualityComparer<string> Default
            {
                get { return _default ?? (_default = new NoCaseComparer()); }
            }
        }
    }
}