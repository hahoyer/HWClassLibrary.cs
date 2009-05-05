using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    [Serializable]
// ReSharper disable ClassNeverInstantiated.Global
    public sealed class SimpleCache<TValueType>
// ReSharper restore ClassNeverInstantiated.Global
    {
        private readonly Func<TValueType> _createValue;
        private bool _isValid;
        private bool _isBusy;
        private TValueType _value;

        public SimpleCache(Func<TValueType> createValue)
        {
            _createValue = createValue;
        }

        public TValueType Value
        {
            get
            {
                ObtainValue(_createValue);
                return _value;
            }
        }

        public void Reset()
        {
            _isValid = false;
        }

        private void ObtainValue(Func<TValueType> createValue)
        {
            Tracer.Assert(!_isBusy);
            if (!_isValid)
            {
                _isBusy = true;
                _value = createValue();
                _isValid = true;
                _isBusy = false;
            }
        }
    }
}