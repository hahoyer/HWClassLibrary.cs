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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    /// <summary>
    ///     Used to control dump.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class DumpClassAttribute : Attribute
    {
        /// <summary>
        ///     override this function to define special dump behaviour of class
        /// </summary>
        /// <param name="top"> true if Dump has been called with that object, false if it is a recursive call within Dump process </param>
        /// <param name="t"> the type to dump. Is the type of any base class of "x" </param>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public abstract string Dump(bool top, Type t, object x);
    }
}