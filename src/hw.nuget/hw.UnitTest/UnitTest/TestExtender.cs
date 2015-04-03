using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hw.UnitTest
{
    public static class TestExtender
    {
        public static bool RunTests(this Assembly rootAssembly)
        {
            return TestRunner.RunTests(rootAssembly);
        }
    }
}