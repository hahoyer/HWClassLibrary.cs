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
    abstract class LogicalExpressionVisitor<T> : Dumpable
    {
        internal T Visit(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Quote:
                    return VisitQuote((UnaryExpression) expression);
                case ExpressionType.Lambda:
                    return VisitLambda(expression);
                case ExpressionType.Equal:
                    return VisitCompareOperation((BinaryExpression) expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression) expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
            }

            NotImplementedMethod(expression);
            return default(T);
        }
        T VisitConstant(ConstantExpression expression) {
            if (expression.Type == typeof(int))
                return VisitConstant((int) expression.Value);
            NotImplementedMethod(expression);
            return default(T);
        }

        internal abstract T VisitConstant(int value);

        T VisitMemberAccess(MemberExpression expression)
        {
            var qualifierExpression = VisitQualifier(expression.Expression);
            var member = expression.Member;
            return qualifierExpression.Member(member);
        }

        IQualifier<T> VisitQualifier(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return VisitQualifier((ParameterExpression) expression);
            }

            NotImplementedMethod(expression);
            return default(IQualifier<T>);
        }

        internal abstract IQualifier<T> VisitQualifier(ParameterExpression expression);

        T VisitCompareOperation(BinaryExpression expression) { return CompareOperation(expression.NodeType, Visit(expression.Left), Visit(expression.Right)); }

        internal abstract T CompareOperation(ExpressionType nodeType, T left, T right);

        T VisitLambda(Expression expression)
        {
            const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var targetType = GetTargetType(expression);
            var methodInfo = GetType().GetMethod("GenericVisitLambda", bf);
            var genericMethod = methodInfo.MakeGenericMethod(targetType);
            return 
                (T)
                genericMethod
                    .Invoke(this, new object[] {expression});
        }

        static Type GetTargetType(Expression expression)
        {
            var type = expression.GetType();
            var genericArguments = type.GetGenericArguments();
            if(type.GetGenericTypeDefinition() != typeof(Expression<>) || genericArguments.Length != 1)
                return null;

            var func = genericArguments[0];
            if(!func.Name.StartsWith("Func"))
                return null;

            var arguments = func.GetGenericArguments();
            if(arguments.Length != 2)
                return null;

            if(arguments[1] != typeof(bool))
                return null;

            return arguments[0];
        }

        [UsedImplicitly]
        protected T GenericVisitLambda<TExpression>(Expression<Func<TExpression, bool>> expression) { return GetLambdaVisitor(expression.Parameters.ToArray()).Visit(expression.Body); }

        protected virtual LogicalExpressionVisitor<T> GetLambdaVisitor(ParameterExpression[] parameters) { return new LambdaVisitor<T>(this, parameters); }

        T VisitQuote(UnaryExpression expression) { return Quote(Visit(expression.Operand)); }

        protected abstract T Quote(T target);
        internal abstract IQualifier<T> Qualifier(int value);
    }

    interface IQualifier<out T>
    {
        T Member(MemberInfo member);
    }

}