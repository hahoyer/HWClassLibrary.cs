using System;
using System.Reflection;
using JetBrains.Annotations;

namespace HWClassLibrary.Debug
{
    /// <summary>
    /// Used to control dump.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct)]
    public abstract class DumpClassAttribute : Attribute
    {
        /// <summary>
        /// override this function to define special dump behaviour of class
        /// </summary>                            
        /// <param name="top">true if Dump has been called with that object, 
        /// false if it is a recursive call within Dump process</param>
        /// <param name="t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public abstract string Dump(bool top, Type t, object x);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public abstract class DumpDataClassAttribute : Attribute
    {
        /// <summary>
        /// override this function to define special dump behaviour of class
        /// </summary>                            
        /// <param name="top">true if Dump has been called with that object, 
        /// false if it is a recursive call within Dump process</param>
        /// <param name="t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public abstract string Dump(Type t, object x);
    }

    /// <summary>
    /// Used to control dump.
    /// Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DumpToStringAttribute : DumpClassAttribute
    {
        /// <summary>
        /// set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name="top">true if Dump has been called with that object, false if it is a recursive call within Dump process</param>
        /// <param name="t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public override string Dump(bool top, Type t, object x)
        {
            return x.ToString();
        }
    }

    /// <summary>
    /// Used to control dump.
    /// Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DumpAttribute : DumpClassAttribute
    {
        private readonly string _name;

        /// <summary>
        /// set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name="top">true if Dump has been called with that object, false if it is a recursive call within Dump process</param>
        /// <param name="t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public override string Dump(bool top, Type t, object x)
        {
            try
            {
                MethodInfo m = t.GetMethod(_name);
                return (string)m.Invoke(x, null);
            }
            catch (Exception)
            {
                return t.ToString();
            }
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public DumpAttribute(string name)
        {
            _name = name;
        }
    }
    /// <summary>
    /// Used to control dump.
    /// Use ToString function
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DumpDataAttribute : DumpDataClassAttribute
    {
        private readonly string _name;

        /// <summary>
        /// set "ToString" as dump behaviour of class
        /// </summary>
        /// <param name="top">true if Dump has been called with that object, false if it is a recursive call within Dump process</param>
        /// <param name="t">the type to dump. Is the type of any base class of "x"</param>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public override string Dump(Type t, object x)
        {
            try
            {
                MethodInfo m = t.GetMethod(_name);
                return (string)m.Invoke(x, null);
            }
            catch (Exception)
            {
                return t.ToString();
            }
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public DumpDataAttribute(string name)
        {
            _name = name;
        }
    }

    /// <summary>
    /// Used to control dump of data element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field), MeansImplicitUse]
    public class IsDumpEnabledAttribute : Attribute
    {
        private readonly bool _value;

        /// <summary>
        /// Swith dump on
        /// </summary>
        /// <param name="value">dump this property or not</param>
        public IsDumpEnabledAttribute(bool value)
        {
            _value = value;
        }

        /// <summary>
        /// Swith dump on
        /// </summary>
        public bool Value { get { return _value; } }
    }

    /// <summary>
    /// Used to control dump of data element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DumpExceptAttribute : Attribute
    {
        private readonly object _exception;

        /// <summary>
        /// Set exception for value tha will not be dumped
        /// </summary>
        /// <param name="exception">dump this property or not</param>
        public DumpExceptAttribute(object exception)
        {
            _exception = exception;
        }

        /// <summary>
        /// obtain exception
        /// </summary>
        public object Exception { get { return _exception; } }
    }
}