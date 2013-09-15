using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Proof
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