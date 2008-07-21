using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public class SimpleCache<ValueType> where ValueType : class
    {
        public ValueType Value;

        public delegate ValueType CreateValue();

        public ValueType Find(CreateValue createValue)
        {
            if(Value == null)
                Value = createValue();
            return Value;
        }
    }
}