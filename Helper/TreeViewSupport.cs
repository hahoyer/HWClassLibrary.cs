using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace HWClassLibrary.Helper.TreeViewSupport
{
    /// <summary>
    /// Attribute to define a subnode for treeview. 
    /// Only for public properties. 
    /// Only the first attribute is considered
    /// </summary>
    public class NodeAttribute : Attribute
    {
        private readonly bool _isEnabled = true;
        private readonly string _title;

        /// <summary>
        /// Default attribute to define a subnode for treeview. 
        /// Property name will be used as title of subnode
        /// </summary>
        /// created 06.02.2007 23:35
        public NodeAttribute()
        {
        }

        /// <summary>
        /// Attribute to enable or disable subnode
        /// </summary>
        /// created 06.02.2007 23:35
        public NodeAttribute(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        /// <summary>
        /// Attribute to define a subnode for treeview with title provided
        /// </summary>
        /// created 06.02.2007 23:35
        public NodeAttribute(string title)
        {
            _title = title;
        }

        /// <summary>
        /// Gets the title. If null, the property name will be used
        /// </summary>
        /// <value>The title.</value>
        /// created 06.02.2007 23:39
        public string Title { get { return _title; } }

        /// <summary>
        /// Gets a value indicating whether this instance is defined.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is defined; otherwise, <c>false</c>.
        /// </value>
        /// created 06.02.2007 23:41
        public bool IsEnabled { get { return _isEnabled; } }
    }

    /// <summary>
    /// Class attribute to define additional node info property, displayed after node title
    /// </summary>
    public class AdditionalNodeInfoAttribute : Attribute
    {
        private readonly string _property;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AdditionalNodeInfoAttribute"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// created 07.02.2007 00:47
        public AdditionalNodeInfoAttribute(string property)
        {
            _property = property;
        }

        /// <summary>
        /// Property to obtain additional node info
        /// </summary>
        public string Property { get { return _property; } }
    }

    public static class Service
    {
        /// <summary>
        /// Creates a treenode.with a given title from an object
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        /// created 06.02.2007 23:26
        private static TreeNode CreateNode(string title, object nodeData)
        {
            var result = new TreeNode(title);
            result.Tag = nodeData;
            return result;
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object. Subnodes are appended until depth given
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data.</param>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        /// created 06.02.2007 23:27
        private static TreeNode CreateTree(string title, object nodeData)
        {
            if (nodeData == null)
                return new TreeNode(title + " = null");

            var xl = nodeData as IList;
            if (xl != null)
                return InternalCreateTree(title, xl);
            var xd = nodeData as IDictionary;
            if (xd != null)
                return InternalCreateTree(title, xd);

            var additionalInfo = GetAdditionalInfo(nodeData);
            return CreateNode(title + additionalInfo, nodeData);
        }

        private static TreeNode InternalCreateTree(string title, IDictionary nodeData)
        {
            var result = CreateNode(title + " Count = " + nodeData.Count, nodeData);
            var i = 0;
            foreach (DictionaryEntry entry in nodeData)
            {
                var treeNode = CreateNode(i + " " + entry.Key, entry);
                treeNode.Nodes.Add(CreateTree("key", entry.Key));
                treeNode.Nodes.Add(CreateTree("value", entry.Value));
                result.Nodes.Add(treeNode);
                i++;
            }
            return result;
        }

        private static TreeNode InternalCreateTree(string title, IList nodeData)
        {
            var result = CreateNode(title + " Count = " + nodeData.Count, nodeData);
            for (var i = 0; i < nodeData.Count; i++)
                result.Nodes.Add(CreateTree(i.ToString(), nodeData[i]));
            return result;
        }

        private static string GetAdditionalInfo(object nodeData)
        {
            var attrs = nodeData.GetType().GetCustomAttributes(typeof (AdditionalNodeInfoAttribute), true);
            if (attrs.Length > 0)
            {
                var attr = (AdditionalNodeInfoAttribute) attrs[0];
                return " " + nodeData.GetType().GetProperty(attr.Property).GetValue(nodeData, null);
            }

            if (nodeData.GetType().Namespace.StartsWith("System"))
                return " = " + nodeData;

            return "";
        }

        private static TreeNode[] CreateNodes(object target)
        {
            var result = new List<TreeNode>();
            result.AddRange(CreateFieldNodes(target));
            result.AddRange(CreatePropertyNodes(target));
            return result.ToArray();
        }

        private static TreeNode[] CreatePropertyNodes(object nodeData)
        {
            var result = new List<TreeNode>();
            foreach (var propertyInfo in nodeData.GetType().GetProperties(DefaultBindingFlags))
            {
                var treeNode = CreateTreeNode(nodeData, propertyInfo);
                if(treeNode != null)
                    result.Add(treeNode);
            }
            return result.ToArray();
        }

        private static TreeNode[] CreateFieldNodes(object nodeData)
        {
            var result = new List<TreeNode>();
            var type = nodeData.GetType();
            foreach (var fieldInfo in type.GetFields(DefaultBindingFlags))
            {
                var treeNode = CreateTreeNode(nodeData, fieldInfo);
                if(treeNode != null)
                    result.Add(treeNode);
            }
            return result.ToArray();
        }

        private static BindingFlags DefaultBindingFlags { get { return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; } }

        delegate object GetObjectDelegate();

        private static TreeNode CreateTreeNode(object nodeData, FieldInfo fieldInfo)
        {
            return CreateTreeNode(fieldInfo, () => fieldInfo.GetValue(nodeData));
        }

        private static TreeNode CreateTreeNode(object nodeData, PropertyInfo propertyInfo)
        {
            return CreateTreeNode(propertyInfo, () => propertyInfo.GetValue(nodeData, null));
        }

        private static TreeNode CreateTreeNode(MemberInfo memberInfo, GetObjectDelegate getValue)
        {
            var attrs = memberInfo.GetCustomAttributes(typeof(NodeAttribute), true);
            if (attrs.Length == 0)
                return null;
            
            var attr = (NodeAttribute) attrs[0];
            if (!attr.IsEnabled)
                return null;

            var value = getValue();
            if (value == null)
                return null;

            return CreateTree(attr.Title ?? memberInfo.Name, value);
        }

        private static void AddNodes(TreeNode target)
        {
            target.Nodes.AddRange(CreateNodes(target.Tag));
        }

        public static void Connect(TreeView treeView, object target)
        {
            treeView.Nodes.AddRange(CreateNodes(target));
            foreach (TreeNode node in treeView.Nodes)
                AddNodes(node);

            treeView.BeforeExpand += treeView_BeforeExpand;
        }

        static void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            AddNodes(e.Node);
        }
    }
}