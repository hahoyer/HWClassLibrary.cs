// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.TreeStructure
{
    /// <summary>
    ///     Attribute to define a subnode for treeview. Only for public properties. Only the first attribute is considered
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse]
    public sealed class NodeAttribute : Attribute
    {
        public readonly string IconKey;

        /// <summary>
        ///     Default attribute to define a subnode for treeview. Property name will be used as title of subnode
        /// </summary>
        /// created 06.02.2007 23:35
        public NodeAttribute() { }

        /// <summary>
        ///     Attribute to define a subnode for treeview with title provided
        /// </summary>
        /// <param name="iconKey"> The icon key. </param>
        /// created 06.02.2007 23:35
        public NodeAttribute(string iconKey) { IconKey = iconKey; }
    }

    /// <summary>
    ///     Class attribute to define additional node info property, displayed after node title
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class AdditionalNodeInfoAttribute : Attribute
    {
        readonly string _property;

        /// <summary>
        ///     Initializes a new instance of the AdditionalNodeInfoAttribute class.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// created 07.02.2007 00:47
        public AdditionalNodeInfoAttribute(string property) { _property = property; }

        /// <summary>
        ///     Property to obtain additional node info
        /// </summary>
        public string Property { get { return _property; } }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SmartNodeAttribute : Attribute
    {
        public static TreeNode Process(TreeNode treeNode)
        {
            treeNode.CreateNodeList();
            switch(treeNode.Nodes.Count)
            {
                case 0:
                    return null;
                case 1:
                    var node = treeNode.Nodes[0];
                    node.Text = treeNode.Text + " \\ " + node.Text;
                    return Process(node);
            }
            return treeNode;
        }
    }

    /// <summary>
    ///     Provides Icon key for treeview
    /// </summary>
    public interface IIconKeyProvider
    {
        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IconKey { get; }
    }

    public interface ITreeNodeSupport
    {
        IEnumerable<TreeNode> CreateNodes();
    }

    public interface IAdditionalNodeInfoProvider
    {
        string AdditionalNodeInfo { get; }
    }
}