using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    /// <summary>
    ///     Used to control dump of data element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DumpExceptAttribute : DumpAttributeBase, IDumpExceptAttribute
    {
        private readonly object _exception;

        /// <summary>
        ///     Set exception for value tha will not be dumped
        /// </summary>
        /// <param name = "exception">dump this property or not</param>
        public DumpExceptAttribute(object exception) { _exception = exception; }

        bool IDumpExceptAttribute.IsException(object v)
        {
            if(_exception == null)
            {
                if(v == null)
                    return true;
                if(v.Equals(DateTime.MinValue))
                    return true;
                return false;
            }
            if(v.Equals(_exception))
                return true;
            return false;
        }
    }
}