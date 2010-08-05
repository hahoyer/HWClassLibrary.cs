using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    [Serializable]
    public sealed class SimpleCache<TValueType>
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
                Ensure();
                return _value;
            }
        }

        public void Ensure()
        {
            Tracer.Assert(!_isBusy);
            if(_isValid)
                return;
            
            _isBusy = true;
            _value = _createValue();
            _isValid = true;
            _isBusy = false;
        }

        public void Reset()
        {
            Tracer.Assert(!_isBusy);
            if (!_isValid)
                return;
            
            _isBusy = true;
            _value = default(TValueType);
            _isValid = false;
            _isBusy = false;
        }

        public bool IsValid { get { return _isValid; } }
    }
}