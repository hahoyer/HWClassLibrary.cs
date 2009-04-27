using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    [Serializable]
// ReSharper disable ClassNeverInstantiated.Global
    public sealed class SimpleCache<TValueType>
// ReSharper restore ClassNeverInstantiated.Global
    {
        private bool _isValid;
        private bool _isBusy;
        public TValueType Value;

        public TValueType Find(Func<TValueType> createValue)
        {
            Tracer.Assert(!_isBusy);
            if(!_isValid)
            {
                _isBusy = true;
                Value = createValue();
                _isValid = true;
                _isBusy = false;
            }
            return Value;
        }
    }
}