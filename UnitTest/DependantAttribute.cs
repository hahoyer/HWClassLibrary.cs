using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.UnitTest
{
    public abstract class DependantAttribute : Attribute
    {
        internal TestType AsTestType(IEnumerable<TestType> testTypes)
        {
            var found = testTypes.Where(x => x.Type == GetType()).ToArray();
            Tracer.Assert(found.Length == 1);
            return found[0];
        }
    }
}