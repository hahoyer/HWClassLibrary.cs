using System.Collections.Generic;

namespace HWClassLibrary.Helper
{
    /// <summary>
    /// Dicionary that does not allow null values
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class DictionaryEx<Key, Value> : Dictionary<Key, Value> 
    {
        public DictionaryEx(DictionaryEx<Key, Value> x)
            : base(x)
        {
            
        }
        public DictionaryEx(DictionaryEx<Key, Value> x, IEqualityComparer<Key> comparer)
            : base(x,comparer)
        {

        }
        public DictionaryEx()
        {

        }

        public DictionaryEx(IEqualityComparer<Key> comparer)
            : base(comparer)
        {

        }
        /// <summary>
        /// Delegate to create the value
        /// </summary>
        /// <returns></returns>
        public delegate Value CreateValue();

        public DictionaryEx<Key, Value> Clone { get { return new DictionaryEx<Key, Value>(this); } }

        /// <summary>
        /// Gets the or add.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createValue">The create value.</param>
        /// <returns></returns>
        /// created 13.01.2007 14:32
        public Value Find(Key key, CreateValue createValue)
        {
            Value result;
            if (TryGetValue(key, out result))
                return result;
            result = createValue();
            Add(key,result);
            return result;
        }

        /// <summary>
        /// Gets the <see cref="T:Value"/> with the specified key
        /// </summary>
        /// <value></value>
        /// created 13.01.2007 15:43
        new public Value this[Key key]
        {
            get
            {
                Value result;
                if (TryGetValue(key, out result))
                    return result;
                return default(Value);
            }
            set { Add(key,value); }
        }

        new public Key[] Keys
        {
            get
            {
                KeyCollection keys = base.Keys;
                Key[]result = new Key[keys.Count];
                int i = 0;
                foreach (Key key in keys)
                    result[i++] = key;
                return result;
            }
        }
    }

    class NoCaseComparer: IEqualityComparer<string>
    {
        private static IEqualityComparer<string> _default;

        ///<summary>
        ///Determines whether the specified objects are equal.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified objects are equal; otherwise, false.
        ///</returns>
        ///
        ///<param name="y">The second object of type T to compare.</param>
        ///<param name="x">The first object of type T to compare.</param>
        public bool Equals(string x, string y)
        {
            return x.ToUpperInvariant() == y.ToUpperInvariant();
        }

        ///<summary>
        ///When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and data structures, such as a hash table.
        ///</summary>
        ///
        ///<returns>
        ///A hash code for the specified object.
        ///</returns>
        ///
        ///<param name="obj">The object for which to get a hash code.</param>
        ///<exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(string obj)
        {
            return EqualityComparer<string>.Default.GetHashCode(obj.ToUpperInvariant());
        }

        public static IEqualityComparer<string> Default
        {
            get
            {
                if(_default == null)
                    _default = new NoCaseComparer();
                return _default;
            }
        }
    }

    public class NoCaseStringDictionary<Value>: DictionaryEx<string ,Value>
    {
        public NoCaseStringDictionary()
            : base(NoCaseComparer.Default)
        {
            
        }
        public NoCaseStringDictionary(NoCaseStringDictionary<Value> x)
            : base(x, NoCaseComparer.Default)
        {
            
        }

        public new NoCaseStringDictionary<Value> Clone { get { return new NoCaseStringDictionary<Value>(this); } }
    }

}