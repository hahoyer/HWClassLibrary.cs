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
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace hw.sqlass
{
    abstract class CollectionExpressionVisitor<T> : ExpressionVisitor<T>
    {
        [UsedImplicitly]
        public abstract T VisitCallWhere(Expression arg0, Expression arg1);

        protected override T Constant(object value)
        {
            NotImplementedMethod(value);
            return default(T);
        }
    }

    interface IExpressionVisitorConstant<out T>
    {
        T Qualifier { get; }
    }

    interface IExpressionVisitorContext
    {}
}