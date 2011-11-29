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
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    static class SelectExtender
    {
        internal static string CreateString(this Expression expression)
        {
            switch(expression.NodeType)
            {
                case ExpressionType.Call:
                    return CreateFromCall((MethodCallExpression) expression);
                case ExpressionType.Constant:
                    return CreateFromConstant((ConstantExpression) expression);
            }

            Tracer.DumpStaticMethodWithData(expression);
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        static string CreateFromConstant(ConstantExpression expression)
        {
            var value = expression.Value;
            var ssr = value as ISelectStructure;
            if(ssr != null)
                return ssr.String;

            Tracer.DumpStaticMethodWithData(value);
            Tracer.TraceBreak();
            throw new NotImplementedException();
        }

        static string CreateFromCall(MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments.ToArray();
            var methodInfo = typeof(CallMethodHandler).GetMethod(method.Name);
            if(methodInfo == null)
            {
                Tracer.DumpData(
                    "\nstatic public SelectExtender "
                    + method.Name
                    + "("
                    + arguments.Length.Array(i => "Expression arg" + i).Format(", ")
                    + ")\n{\nNotImplementedFunction("
                    + arguments.Length.Array(i => "arg" + i).Format(", ")
                    + ");\nreturn null;\n}\n\n"
                    + arguments.Length.Array(i => "arg" + i + " = " + Tracer.Dump(arguments[i])).Format("\n")
                    );
                Tracer.TraceBreak();
                throw new MissingMethodException(method.Name);
            }
            return (string) methodInfo.Invoke(null, arguments.Cast<object>().ToArray());
        }

        static class CallMethodHandler
        {
            [UsedImplicitly]
            public static string Where(Expression arg0, Expression arg1)
            {
                return CreateString(arg0) + " where " + LogicalExtender.CreateString(arg1);
            }
        }

        static class LogicalExtender
        {
            internal static string CreateString(Expression expression)
            {
                switch(expression.NodeType)
                {
                    case ExpressionType.Quote:
                        return CreateFromQuote((UnaryExpression) expression);
                    case ExpressionType.Lambda:
                        return CreateFromLambda(expression);
                }

                Tracer.DumpStaticMethodWithData(expression);
                Tracer.TraceBreak();
                throw new NotImplementedException();
            }

            static string CreateFromLambda(Expression expression)
            {
                const BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                var result = (ILogicalStructure) 
                    typeof(LogicalExtender)
                    .GetMethod("GenericCreateFromLambda", bf)
                    .MakeGenericMethod(GetTargetType(expression))
                    .Invoke(null, new object[] {expression});
                Tracer.DumpStaticMethodWithData(expression);
                Tracer.TraceBreak();
                throw new NotImplementedException();
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
            static string GenericCreateFromLambda<T>(Expression<Func<T, bool>> expression)
            {
                var p = expression.Parameters[0];
                var b = expression.Body;
                Tracer.DumpStaticMethodWithData(expression);
                Tracer.TraceBreak();
                throw new NotImplementedException();
            }

            static string CreateFromQuote(UnaryExpression expression) { return CreateString(expression.Operand).Quote(); }
        }
    }
}