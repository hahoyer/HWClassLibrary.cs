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

using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    static class Handler<T>
    {
        [UsedImplicitly]
        public static T Single(Expression table, Expression condition)
        {
            var list = new StringVisitor().VisitCallWhere(table, condition);
            Tracer.DumpStaticMethodWithData(table, condition);
            Tracer.TraceBreak();
            return default(T);
        }
    }
}