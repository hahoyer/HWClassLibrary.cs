using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    /// <summary>
    ///     Used to control dump.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class DumpClassAttribute : Attribute
    {
        /// <summary>
        ///     override this function to define special dump behaviour of class
        /// </summary>
        /// <param name = "top">true if Dump has been called with that object, 
        ///     false if it is a recursive call within Dump process</param>
        /// <param name = "t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name = "x">the object to dump</param>
        /// <returns></returns>
        public abstract string Dump(bool top, Type t, object x);
    }
}