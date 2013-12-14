using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Forms
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public class NodeNameAttribute : Attribute
    {
        readonly string _property;

        /// <summary>
        ///     Initializes a new instance of the AdditionalNodeInfoAttribute class.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// created 07.02.2007 00:47
        public NodeNameAttribute(string property) { _property = property; }

        /// <summary>
        ///     Property to obtain additional node info
        /// </summary>
        public string Property { get { return _property; } }
    }

    interface INodeNameProvider
    {
        string Value(string name);
    }

    interface ITreeNodeProbeSupport
    {
        bool IsEmpty { get; }
    }

    sealed class LazyNode
    {
        public object Target;
    }
}