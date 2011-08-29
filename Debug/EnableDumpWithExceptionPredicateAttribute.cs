//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace HWClassLibrary.Debug
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class EnableDumpWithExceptionPredicateAttribute : DumpEnabledAttribute, IDumpExceptAttribute
    {
        readonly string _predicate;
        public EnableDumpWithExceptionPredicateAttribute(string predicate = "")
            : base(true) { _predicate = predicate == ""? "IsDumpException":predicate; }
        bool IDumpExceptAttribute.IsException(object target)
        {
            try
            {
                return (bool)
                       target
                           .GetType()
                           .GetMethod(_predicate)
                           .Invoke(target, null);
            }
            catch(Exception e)
            {
                Tracer.AssertionFailed("Exception when calling " + target.GetType().PrettyName() + _predicate, () => e.Message);
                return false;
            }
        }
    }

}