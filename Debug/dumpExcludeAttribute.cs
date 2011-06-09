using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class DumpDataClassAttribute : Attribute
    {
        public abstract string Dump(Type t, object x);
    }

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

    /// <summary>
    ///     Used to control dump.
    ///     Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class DumpAttribute : DumpClassAttribute
    {
        private readonly string _name;

        /// <summary>
        ///     set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name = "top">true if Dump has been called with that object, false if it is a recursive call within Dump process</param>
        /// <param name = "t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name = "x">the object to dump</param>
        /// <returns></returns>
        public override string Dump(bool top, Type t, object x)
        {
            try
            {
                var m = t.GetMethod(_name);
                return (string) m.Invoke(x, null);
            }
            catch(Exception)
            {
                return t.ToString();
            }
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name = "name"></param>
        public DumpAttribute(string name) { _name = name; }
    }

    /// <summary>
    ///     Used to control dump.
    ///     Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DumpDataAttribute : DumpDataClassAttribute
    {
        private readonly string _name;

        public override string Dump(Type t, object x)
        {
            try
            {
                var m = t.GetMethod(_name);
                return (string) m.Invoke(x, null);
            }
            catch(Exception)
            {
                return t.ToString();
            }
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name = "name"></param>
        public DumpDataAttribute(string name) { _name = name; }
    }

    public abstract class DumpEnabledAttributeBase : Attribute
    {
        private readonly bool _value;

        protected DumpEnabledAttributeBase(bool value) { _value = value; }

        public bool Value { get { return _value; } }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field), MeansImplicitUse]
    public sealed class EnableDumpAttribute : DumpEnabledAttributeBase
    {
        public EnableDumpAttribute()
            : base(true) {}
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DisableDumpAttribute : DumpEnabledAttributeBase
    {
        public DisableDumpAttribute()
            : base(false) { }
    }

    /// <summary>
    ///     Used to control dump of data element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DumpExceptAttribute : Attribute
    {
        private readonly object _exception;

        /// <summary>
        ///     Set exception for value tha will not be dumped
        /// </summary>
        /// <param name = "exception">dump this property or not</param>
        public DumpExceptAttribute(object exception) { _exception = exception; }

        /// <summary>
        ///     obtain exception
        /// </summary>
        public object Exception { get { return _exception; } }
    }
}