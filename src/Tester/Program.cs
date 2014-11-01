using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Tests.CompilerTool;
using hw.UnitTest;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRunner.IsModeErrorFocus = true;
            //Parser.ParenthesisSuffixAndPrefixAndSequence();
            TestRunner.IsModeErrorFocus = false;
            TestRunner.RunTests(Assembly.GetExecutingAssembly());
        }
    }
}