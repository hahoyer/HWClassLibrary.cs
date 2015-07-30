using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.Helper
{
    [Serializable]
    public sealed class ValueCache<TValueType>
    {
        readonly Func<TValueType> _createValue;
        bool _isValid;
        bool _isBusy;
        TValueType _value;

        public ValueCache(Func<TValueType> createValue) { _createValue = createValue; }

        public TValueType Value
        {
            get
            {
                Ensure();
                return _value;
            }
        }

        void Ensure()
        {
            Tracer.Assert(!_isBusy);
            if(_isValid)
                return;

            _isBusy = true;
            try
            {
                _value = _createValue();
                _isValid = true;
            }
            finally
            {
                _isBusy = false;
            }
        }

        void Reset()
        {
            Tracer.Assert(!_isBusy);
            if(!_isValid)
                return;

            _isBusy = true;
            _value = default(TValueType);
            _isValid = false;
            _isBusy = false;
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if(value)
                    Ensure();
                else
                    Reset();
            }
        }

        [EnableDumpExcept(false)]
        public bool IsBusy { get { return _isBusy; } }
    }
}