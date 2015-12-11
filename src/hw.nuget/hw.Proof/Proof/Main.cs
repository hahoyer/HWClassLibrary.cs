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
using hw.DebugFormatter;

namespace hw.Proof
{
    public static class Main
    {
        public static void Run()
        {
            var statement = new Holder(@"
a elem Integer;
b elem Integer; 
c elem Integer; 
a^2 + b^2 = c^2;
a gcd b = 1;
c + a = x;
c - a = y 
").Statement;
            Tracer.FlaggedLine("Statement: " + statement.SmartDump(), FilePositionTag.Test);
            statement = statement.IsolateAndReplace(3);
            Tracer.FlaggedLine("Statement: " + statement.SmartDump(), FilePositionTag.Test);
            statement = statement.IsolateAndReplace(3);
            Tracer.FlaggedLine("Statement: " + statement.SmartDump(), FilePositionTag.Test);
            statement = statement.IsolateAndReplace(3);
            Tracer.FlaggedLine("Statement: " + statement.SmartDump(), FilePositionTag.Test);
        }

        internal static readonly TokenFactory TokenFactory = TokenFactory.Instance;
    }
}