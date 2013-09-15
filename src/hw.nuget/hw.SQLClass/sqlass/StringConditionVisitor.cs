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
using hw.Helper;

namespace hw.sqlass
{
    sealed class StringConditionVisitor : LogicalExpressionVisitor<string>
    {
        protected override string Constant(object value) { return value.ToString(); }
        protected override string CompareOperation(ExpressionType nodeType, string left, string right) { return "({0}){1}({2})".ReplaceArgs(left, Operator(nodeType), right); }

        string MemberName(Type type, string name)
        {
            if(type.GetInterfaces().Contains(typeof(ISQLSupportProvider)))
                return name;
            NotImplementedMethod(type, name);
            return null;
        }

        protected override string VisitMemberAccess(MemberExpression expression)
        {
            var member = expression.Member;
            if(member.DeclaringType.Is<ISQLSupportProvider>())
            {
                var qualifier = Visit(expression.Expression);
                return qualifier + "." + member.Name;
            }

            NotImplementedMethod(expression);
            return null;
        }

        protected override string Parameter(string name) { return name; }

        string Operator(ExpressionType nodeType)
        {
            switch(nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
            }
            NotImplementedMethod(nodeType);
            return null;
        }
        protected override string Visit(IExpressionVisitorConstant<string> target)
        {
            NotImplementedMethod(target);
            return null;
        }
    }
}