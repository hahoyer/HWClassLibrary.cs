//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace HWClassLibrary.Helper
{
    /// <summary>
    ///     Dicionary that does not allow null values
    /// </summary>
    /// <typeparam name = "TKey"></typeparam>
    /// <typeparam name = "TValue"></typeparam>
    [AdditionalNodeInfo("NodeDump")]
    public class DictionaryEx<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _createValue;

        public DictionaryEx(Func<TKey, TValue> createValue) { _createValue = createValue; }

        public DictionaryEx(TValue defaultValue, Func<TKey, TValue> createValue)
        {
            DefaultValue = defaultValue;
            _createValue = createValue;
        }

        public DictionaryEx(IEqualityComparer<TKey> comparer, Func<TKey, TValue> createValue)
            : base(comparer) { _createValue = createValue; }

        public DictionaryEx(DictionaryEx<TKey, TValue> x, IEqualityComparer<TKey> comparer)
            : base(x, comparer) { _createValue = x._createValue; }

        public DictionaryEx(DictionaryEx<TKey, TValue> x)
            : base(x) { _createValue = x._createValue; }

        public DictionaryEx() { _createValue = ThrowKeyNotFoundException; }

        private static TValue ThrowKeyNotFoundException(TKey key) { throw new KeyNotFoundException(key.ToString()); }


        public DictionaryEx<TKey, TValue> Clone { get { return new DictionaryEx<TKey, TValue>(this); } }

        [DisableDump]
        public string NodeDump
        {
            get
            {
                var genericArguments = GetType().GetGenericArguments();
                return "DictionaryEx<" + genericArguments[0].FullName + "," + genericArguments[1].FullName + ">[" +
                       Count + "]";
            }
        }

        /// <summary>
        ///     Gets the or add.
        /// </summary>
        /// <param name = "key">The key.</param>
        /// <returns></returns>
        /// created 13.01.2007 14:32
        public TValue Find(TKey key)
        {
            TValue result;
            if(TryGetValue(key, out result))
            {
                Tracer.Assert(!Equals(result, DefaultValue));
                return result;
            }
            base[key] = DefaultValue;
            result = _createValue(key);
            base[key] = result;
            return result;
        }

        public readonly TValue DefaultValue;

        /// <summary>
        ///     Gets the value with the specified key
        /// </summary>
        /// <value></value>
        /// created 13.01.2007 15:43
        public new TValue this[TKey key]
        {
            get
            {
                TValue result;
                if(TryGetValue(key, out result))
                    return result;
                return default(TValue);
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
    }

    internal sealed class NoCaseComparer : IEqualityComparer<string>
    {
        private static IEqualityComparer<string> _default;

        ///<summary>
        ///    Determines whether the specified objects are equal.
        ///</summary>
        ///<returns>
        ///    true if the specified objects are equal; otherwise, false.
        ///</returns>
        ///<param name = "y">The second object of type T to compare.</param>
        ///<param name = "x">The first object of type T to compare.</param>
        public bool Equals(string x, string y) { return x.ToUpperInvariant() == y.ToUpperInvariant(); }

        ///<summary>
        ///    When overridden in a derived class, serves as a hash function for the specified object for hashing algorithms and data structures, such as a hash table.
        ///</summary>
        ///<returns>
        ///    A hash code for the specified object.
        ///</returns>
        ///<param name = "obj">The object for which to get a hash code.</param>
        ///<exception cref = "T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(string obj) { return EqualityComparer<string>.Default.GetHashCode(obj.ToUpperInvariant()); }

        public static IEqualityComparer<string> Default
        {
            get { return _default ?? (_default = new NoCaseComparer()); }
        }
    }

    public class NoCaseStringDictionary<TValue> : DictionaryEx<string, TValue>
    {
        public NoCaseStringDictionary(Func<string, TValue> createValue)
            : base(NoCaseComparer.Default, createValue) { }

// ReSharper disable SuggestBaseTypeForParameter
        public NoCaseStringDictionary(NoCaseStringDictionary<TValue> x)
// ReSharper restore SuggestBaseTypeForParameter
            : base(x, NoCaseComparer.Default) { }

        public new NoCaseStringDictionary<TValue> Clone { get { return new NoCaseStringDictionary<TValue>(this); } }
    }
}