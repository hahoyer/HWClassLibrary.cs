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
    sealed class StringVisitor : CollectionExpressionVisitor<string>
    {
        protected override string Visit(IExpressionVisitorConstant<string> target) { return target.Qualifier; }

        public override string VisitCallWhere(Expression arg0, Expression arg1)
        {
            var context = new WhereContextVisitor(arg0);

            return Visit(arg0)
                   + " where "
                   + context.Visit(arg1);
        }
    }

    sealed class WhereContextVisitor : LogicalExpressionVisitor<string>
    {
        [EnableDump]
        readonly Expression _expression;
        public WhereContextVisitor(Expression expression) { _expression = expression; }

        internal override string VisitConstant(int value) { return value.ToString(); }

        internal override IQualifier<string> VisitQualifier(ParameterExpression expression)
        {
            NotImplementedMethod(expression);
            return null;
        }

        internal override string CompareOperation(ExpressionType nodeType, string left, string right) { return "({0}){1}({2})".ReplaceArgs(left, Operator(nodeType), right); }

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
        protected override string Quote(string target) { return target; }

        internal override IQualifier<string> Qualifier(int value)
        {
            if(value == 0)
                return MetaDataQualifier(_expression);
            NotImplementedMethod(value);
            return null;
        }

        IQualifier<string> MetaDataQualifier(Expression expression)
        {
            var mqf = new MetaDataQualifierFinder();
            return mqf.Visit(expression);
        }
    }

    sealed class MetaDataQualifierFinder : CollectionExpressionVisitor<IQualifier<string>>
    {
        protected override IQualifier<string> Visit(IExpressionVisitorConstant<IQualifier<string>> target) { return target.Qualifier; }

        public override IQualifier<string> VisitCallWhere(Expression arg0, Expression arg1) { return Visit(arg0); }
    }
}