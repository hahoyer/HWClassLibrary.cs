using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    /// <summary>
    ///     Used to control dump.
    ///     Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class DumpToStringAttribute : DumpClassAttribute
    {
        /// <summary>
        ///     set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name = "top">true if Dump has been called with that object, false if it is a recursive call within Dump process</param>
        /// <param name = "t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name = "x">the object to dump</param>
        /// <returns></returns>
        public override string Dump(bool top, Type t, object x) { return x.ToString(); }
    }
}