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
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    abstract class LogicalExpressionVisitor<T> : ExpressionVisitor<T>
    {
        internal override T Visit(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Convert:
                    return VisitConvert((UnaryExpression)expression);
                case ExpressionType.Equal:
                    return VisitCompareOperation((BinaryExpression) expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression) expression);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression) expression);
            }
            return base.Visit(expression);
        }


        protected override T VisitConstant(Type type, object value)
        {
            if(type == typeof(int))
                return Constant((int) value);
            return base.VisitConstant(type, value);
        }

        T VisitCompareOperation(BinaryExpression expression) { return CompareOperation(expression.NodeType, Visit(expression.Left), Visit(expression.Right)); }
        T VisitConvert(UnaryExpression expression) { return Visit(expression.Operand); }
        T VisitMemberAccess(MemberExpression expression) { return MemberAccess(Visit(expression.Expression), expression.Member); }
        T VisitParameter(ParameterExpression expression) { return Parameter(expression.Name); }

        [UsedImplicitly]
        public T VisitCallEquals(Expression arg0, Expression arg1) { return CompareOperation(ExpressionType.Equal, Visit(arg0), Visit(arg1)); }

        protected abstract T CompareOperation(ExpressionType nodeType, T left, T right);
        protected abstract T Constant(int value);
        protected abstract T MemberAccess(T qualifier, MemberInfo member);
        protected abstract T Parameter(string name);
    }

    interface IQualifier<out T>
    {}
}