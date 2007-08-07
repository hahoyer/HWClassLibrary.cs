using System;
using System.Reflection;

namespace HWClassLibrary.Debug
{
    /// <summary>
    /// Used to control dump.
    /// </summary>
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

    /// <summary>
    /// Used to control dump.
    /// Use ToString function
    /// </summary>
    public class dumpToStringAttribute : DumpClassAttribute
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
    public class dumpAttribute : DumpClassAttribute
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
                return (string) m.Invoke(x, null);
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
        public dumpAttribute(string name)
        {
            _name = name;
        }
    }

    /// <summary>
    /// Used to control dump of data element
    /// </summary>
    public class DumpDataAttribute : Attribute
    {
        private readonly bool _dump;

        /// <summary>
        /// Swith dump on
        /// </summary>
        /// <param name="dump">dump this property or not</param>
        public DumpDataAttribute(bool dump)
        {
            _dump = dump;
        }

        /// <summary>
        /// Swith dump on
        /// </summary>
        public bool Dump { get { return _dump; } }
    }

    /// <summary>
    /// Used to control dump of data element
    /// </summary>
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