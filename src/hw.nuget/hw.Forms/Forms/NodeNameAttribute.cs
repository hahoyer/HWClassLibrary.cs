using System;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Forms
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public class NodeNameAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the AdditionalNodeInfoAttribute class.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// created 07.02.2007 00:47
        public NodeNameAttribute(string property) => Property = property;

        /// <summary>
        ///     Property to obtain additional node info
        /// </summary>
        public string Property { get; }
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