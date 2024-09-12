using System;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Forms
{
    /// <summary>
    ///     Attribute to define a subnode for treeview. Only for public properties. Only the first attribute is considered
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse]
    public sealed class NodeAttribute : Attribute
    {
        public readonly string IconKey;
        public readonly string Name;

        /// <summary>
        ///     Attribute to define a subnode for treeview with title provided
        /// </summary>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="name"></param>
        /// created 06.02.2007 23:35
        public NodeAttribute(string iconKey = null, string name = null)
        {
            IconKey = iconKey;
            Name = name;
        }
    }
}