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

namespace hw.DebugFormatter
{
    /// <summary>
    ///     Used to control dump. Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class DumpAttribute : DumpClassAttribute
    {
        readonly string _name;

        /// <summary>
        ///     set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name="t"> the type to dump. Is the type of any base class of "x" </param>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public override string Dump(Type t, object x)
        {
            try
            {
                var m = t.GetMethod(_name);
                return (string) m.Invoke(x, null);
            }
            catch(Exception)
            {
                return t.ToString();
            }
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="name"> </param>
        public DumpAttribute(string name) { _name = name; }
    }
}