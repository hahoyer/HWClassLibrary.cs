#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

#endregion

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
            _value = _createValue();
            _isValid = true;
            _isBusy = false;
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