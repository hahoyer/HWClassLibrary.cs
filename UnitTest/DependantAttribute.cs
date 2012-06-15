#region Copyright (C) 2012

//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using HWClassLibrary.Debug;

namespace HWClassLibrary.UnitTest
{
    public abstract class DependantAttribute : Attribute
    {
        internal TestType AsTestType(IEnumerable<TestType> testTypes)
        {
            var found = testTypes.Where(x => x.Type == GetType()).ToArray();
            Tracer.Assert(found.Length == 1);
            return found[0];
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LowPriority : Attribute
    {}
}