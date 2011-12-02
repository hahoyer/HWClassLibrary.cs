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
using System.Reflection;
using HWClassLibrary.Debug;

namespace HWClassLibrary.sqlass
{
    sealed class LambdaVisitor<T> : LogicalExpressionVisitor<T>
    {
        [EnableDump]
        readonly LogicalExpressionVisitor<T> _parent;
        [EnableDump]
        readonly ParameterExpression[] _parameters;
        internal LambdaVisitor(LogicalExpressionVisitor<T> parent, ParameterExpression[] parameters)
        {
            _parent = parent;
            _parameters = parameters;
        }

        internal override T CompareOperation(ExpressionType nodeType, T left, T right) { return _parent.CompareOperation(nodeType, left, right); }

        protected override LogicalExpressionVisitor<T> GetLambdaVisitor(ParameterExpression[] parameters)
        {
            NotImplementedMethod(parameters);
            return null;
        }

        protected override T Quote(T target)
        {
            NotImplementedMethod(target);
            return default(T);
        }
        internal override IQualifier<T> Qualifier(int value)
        {
            NotImplementedMethod(value);
            return null;
        }

        internal override T VisitConstant(int value) { return _parent.VisitConstant(value); }
        
        internal override IQualifier<T> VisitQualifier(ParameterExpression expression)
        {
            int? index = null;
            index = FindParameter(expression);
            if(index == null)
                return _parent.VisitQualifier(expression);

            return _parent.Qualifier(index.Value);
        }

        int? FindParameter(ParameterExpression expression)
        {
            for(var i = 0; i < _parameters.Length; i++)
                if(_parameters[i] == expression)
                    return i;
            return null;
        }
    }
}