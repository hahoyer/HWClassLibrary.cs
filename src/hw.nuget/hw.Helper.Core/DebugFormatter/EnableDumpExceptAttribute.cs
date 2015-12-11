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

namespace hw.DebugFormatter
{
    /// <summary>
    ///     Used to control dump of data element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class EnableDumpExceptAttribute : DumpExceptAttribute, IDumpExceptAttribute
    {
        /// <summary>
        ///     Set exception for value tha will not be dumped
        /// </summary>
        /// <param name="exception"> dump this property or not </param>
        public EnableDumpExceptAttribute(object exception)
            : base(exception) { }

        bool IDumpExceptAttribute.IsException(object v) { return IsException(v); }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    public abstract class DumpExceptAttribute : DumpAttributeBase
    {
        readonly object _exception;

        protected DumpExceptAttribute(object exception) { _exception = exception; }

        protected bool IsException(object v)
        {
            if(_exception == null)
            {
                if(v == null)
                    return true;
                if(v.Equals(DateTime.MinValue))
                    return true;
                return false;
            }
            if(v.Equals(_exception))
                return true;
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class DisableDumpExceptAttribute : DumpExceptAttribute, IDumpExceptAttribute
    {
        /// <summary>
        ///     Set exception for value tha will not be dumped
        /// </summary>
        /// <param name="exception"> dump this property or not </param>
        public DisableDumpExceptAttribute(object exception)
            : base(exception) { }
        bool IDumpExceptAttribute.IsException(object v) { return !IsException(v); }
    }
}