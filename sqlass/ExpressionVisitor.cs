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
                Tracer.FlaggedLine(
                    "\n[UsedImplicitly]\ninternal T VisitCall"
                    + method.Name
                    + "("
                    + arguments.Length.Array(i => "Expression arg" + i).Format(", ")
                    + ")\n{\nNotImplementedFunction("
                    + arguments.Length.Array(i => "arg" + i).Format(", ")
                    + ");\nreturn default(T);\n}\n\n"
                    + arguments.Length.Array(i => "arg" + i + " = " + Tracer.Dump(arguments[i])).Format("\n")
                    );
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

            NotImplementedMethod(type, value);
            return default(T);
        }

        protected abstract T Visit(IExpressionVisitorConstant<T> target);
    }
}