using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class DumpDataClassAttribute : Attribute
    {
        public abstract string Dump(Type t, object x);
    }
}