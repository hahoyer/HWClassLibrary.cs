using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.UnitTest
{
    public abstract class DependantAttribute : Attribute
    {
        internal TestType AsTestType(IEnumerable<TestType> testTypes)
        {
            return testTypes.SingleOrDefault(x => x.Type == GetType());
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LowPriority : Attribute {}
}