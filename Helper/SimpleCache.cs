using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public class SimpleCache<ValueType> where ValueType : class
    {
        public ValueType Value;

        public ValueType Find(Func<ValueType> createValue)
        {
            if(Value == null)
                Value = createValue();
            return Value;
        }
    }
}