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
    abstract class LogicalExpressionVisitor<T> : Dumpable
    {
        internal T Visit(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitCompareOperation((BinaryExpression) expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression) expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression) expression);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);
            }
            Tracer.FlaggedLine(expression.NodeType.ToString());
            NotImplementedMethod(expression);
            return default(T);
        }
        
        T VisitParameter(ParameterExpression expression) { return Parameter(expression.Name); }

        T VisitConstant(ConstantExpression expression)
        {
            if(expression.Type == typeof(int))
                return Constant((int) expression.Value);
            NotImplementedMethod(expression);
            return default(T);
        }

        T VisitMemberAccess(MemberExpression expression) { return MemberAccess(Visit(expression.Expression), expression.Member); }
        
        T VisitCompareOperation(BinaryExpression expression) { return CompareOperation(expression.NodeType, Visit(expression.Left), Visit(expression.Right)); }

        protected abstract T Constant(int value);
        protected abstract T MemberAccess(T qualifier, MemberInfo member);
        protected abstract T CompareOperation(ExpressionType nodeType, T left, T right);
        protected abstract T Parameter(string name);
    }

    interface IQualifier<out T>
    {}
}