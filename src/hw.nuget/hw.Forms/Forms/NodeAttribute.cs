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
using JetBrains.Annotations;

namespace hw.Forms
{
    /// <summary>
    ///     Attribute to define a subnode for treeview. Only for public properties. Only the first attribute is considered
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse]
    public sealed class NodeAttribute : Attribute
    {
        public readonly string IconKey;
        public readonly string Name;

        /// <summary>
        ///     Attribute to define a subnode for treeview with title provided
        /// </summary>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="name"></param>
        /// created 06.02.2007 23:35
        public NodeAttribute(string iconKey = null, string name = null)
        {
            IconKey = iconKey;
            Name = name;
        }
    }
}