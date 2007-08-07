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
        public DictionaryEx()
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
    }
}