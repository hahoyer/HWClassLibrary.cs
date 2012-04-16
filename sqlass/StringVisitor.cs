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
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass
{
    sealed class StringVisitor : CollectionExpressionVisitor<string>
    {
        protected override string Visit(IExpressionVisitorConstant<string> target) { return target.Qualifier; }

        public override string VisitCallWhere(Expression arg0, Expression arg1)
        {
            var context = new WhereContextVisitor().Visit(arg1);
            return "select * from ({0}) {1} where ({2})".ReplaceArgs(Visit(arg0), context.Item1, context.Item2);
        }
    }
}