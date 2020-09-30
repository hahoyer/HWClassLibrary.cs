using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Helper
{
    [PublicAPI]
    [Serializable]
    public sealed class ValueCache<TValueType>
    {
        [EnableDumpExcept(false)]
        public bool IsBusy { get; private set; }

        readonly Func<TValueType> CreateValue;
        TValueType Data;
        bool ValidityState;

        public ValueCache(Func<TValueType> createValue) => CreateValue = createValue;

        public TValueType Value
        {
            get
            {
                Ensure();
                return Data;
            }
        }

        public bool IsValid
        {
            get => ValidityState;
            set
            {
                if(value)
                    Ensure();
                else
                    Reset();
            }
        }

        void Ensure()
        {
            Tracer.Assert(!IsBusy, "Recursive attempt to get value.");
            if(ValidityState)
                return;

            IsBusy = true;
            try
            {
                Data = CreateValue();
                ValidityState = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        void Reset()
        {
            Tracer.Assert(!IsBusy, "Attempt to reset value during getting value.");
            if(!ValidityState)
                return;

            IsBusy = true;
            Data = default(TValueType);
            ValidityState = false;
            IsBusy = false;
        }
    }

    public sealed class ValueCache : Dictionary<object, object>
    {
        public interface IContainer
        {
            ValueCache Cache { get; }
        }
    }
}