using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Helper
{
    [Serializable]
    public sealed class ValueCache<TValueType>
    {
        readonly Func<TValueType> _createValue;
        bool _isValid;
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
            Tracer.Assert(!IsBusy, "Recursive attemt to get value.");
            if (_isValid)
                return;

            IsBusy = true;
            try
            {
                _value = _createValue();
                _isValid = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        void Reset()
        {
            Tracer.Assert(!IsBusy, "Attempt to reset value during getting value.");
            if (!_isValid)
                return;

            IsBusy = true;
            _value = default(TValueType);
            _isValid = false;
            IsBusy = false;
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
        public bool IsBusy { get; private set; }
    }

    public sealed class ValueCache : Dictionary<object, object>
    {
        public interface IContainer
        {
            ValueCache Cache { get; }
        }
    }
}