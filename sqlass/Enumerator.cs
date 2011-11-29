// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.sqlass
{
    sealed class Enumerator<TElement> : Dumpable, IEnumerator<TElement>
    {
        readonly IEnumerator _index;
        readonly DbDataReader _reader;
        public Enumerator(DbDataReader reader)
        {
            _reader = reader;
            _index = _reader.GetEnumerator();
        }
        void IDisposable.Dispose() { NotImplementedMethod(); }
        bool IEnumerator.MoveNext() { return _index.MoveNext(); }
        void IEnumerator.Reset() { _index.Reset(); }
        TElement IEnumerator<TElement>.Current { get { return Context.CreateObject<TElement>(_index.Current); } }
        object IEnumerator.Current { get { return ((IEnumerator<TElement>) this).Current; } }
    }
}