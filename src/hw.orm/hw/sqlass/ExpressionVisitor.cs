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
using hw.Helper;

namespace hw.sqlass
{
    abstract class ExpressionVisitor<T> : Dumpable
    {
        internal virtual T Visit(Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Call:
                    return VisitCall((MethodCallExpression) expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression) expression);
            }
            Tracer.FlaggedLine(expression.NodeType.ToString());
            NotImplementedMethod(expression);
            return default(T);
        }

        T VisitCall(MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments.ToArray();
            if(!method.IsStatic)
                arguments = new[] {expression.Object}.Union(arguments).ToArray();

            var methodInfo = GetType().GetMethod("VisitCall" + method.Name);
            if(methodInfo == null)
            {
                Tracer.FlaggedLine("\n[UsedImplicitly]\ninternal T VisitCall" + method.Name + "(" + arguments.Length.Select(i => "Expression arg" + i).Stringify(", ") + ")\n{\nNotImplementedFunction(" + arguments.Length.Select(i => "arg" + i).Stringify(", ") + ");\nreturn default(T);\n}\n\n" + arguments.Length.Select(i => "arg" + i + " = " + Tracer.Dump(arguments[i])).Stringify("\n"));
                Tracer.TraceBreak();
                throw new MissingMethodException(method.Name);
            }
            return (T) methodInfo.Invoke(this, arguments.Cast<object>().ToArray());
        }

        T VisitConstant(ConstantExpression expression) { return VisitConstant(expression.Type, expression.Value); }

        protected virtual T VisitConstant(Type type, object value)
        {
            var ssr = value as IExpressionVisitorConstant<T>;
            if(ssr != null)
                return Visit(ssr);

            if(type.Name.StartsWith("<>"))
                return VisitRuntime(type, value);

            NotImplementedMethod(type, value);
            return default(T);
        }
        T VisitRuntime(Type type, object value)
        {
            var field = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Single();
            return Constant(field.GetValue(value));
        }
        protected abstract T Constant(object value);
        protected abstract T Visit(IExpressionVisitorConstant<T> target);
    }
}