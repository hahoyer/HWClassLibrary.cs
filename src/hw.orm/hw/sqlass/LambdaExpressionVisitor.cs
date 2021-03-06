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
using System.Reflection;
using hw.Debug;
using JetBrains.Annotations;

namespace hw.sqlass
{
    abstract class LambdaExpressionVisitor<T> : ExpressionVisitor<T>
    {
        internal override T Visit(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Quote:
                    return VisitQuote((UnaryExpression) expression);
                case ExpressionType.Lambda:
                    return VisitLambda(expression);
            }
            Tracer.FlaggedLine(expression.NodeType.ToString());
            NotImplementedMethod(expression);
            return default(T);
        }

        T VisitQuote(UnaryExpression expression) { return Visit(expression.Operand); }

        T VisitLambda(Expression expression)
        {
            const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var targetType = GetTargetType(expression);
            var methodInfo = GetType().GetMethod("GenericVisitLambda", bf);
            var genericMethod = methodInfo.MakeGenericMethod(targetType);
            return (T) genericMethod.Invoke(this, new object[] {expression});
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
        protected T GenericVisitLambda<TExpression>(Expression<Func<TExpression, bool>> expression) { return VisitLambda(expression.Parameters.ToArray(), expression.Body); }
        protected abstract T VisitLambda(ParameterExpression[] parameters, Expression body);

        protected override sealed T Constant(object value)
        {
            NotImplementedMethod(value);
            return default(T);
        }

        protected override sealed T Visit(IExpressionVisitorConstant<T> target)
        {
            NotImplementedMethod(target);
            return default(T);
        }
    }
}