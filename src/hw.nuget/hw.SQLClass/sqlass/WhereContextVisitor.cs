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
using hw.Debug;

namespace hw.sqlass
{
    sealed class WhereContextVisitor : LambdaExpressionVisitor<Tuple<string, string>>
    {
        protected override Tuple<string, string> VisitLambda(ParameterExpression[] parameters, Expression body)
        {
            Tracer.Assert(parameters.Length == 1);
            return new Tuple<string, string>(parameters[0].Name, new StringConditionVisitor().Visit(body));
        }
    }
}