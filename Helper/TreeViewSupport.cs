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
        public string Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is defined.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is defined; otherwise, <c>false</c>.
        /// </value>
        /// created 06.02.2007 23:41
        public bool IsEnabled
        {
            get { return _isEnabled; }
        }
    }

    /// <summary>
    /// Class attribute to define additional node info property, displayed after node title
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct|AttributeTargets.Class)]
    public class AdditionalNodeInfoAttribute : Attribute
    {
        private readonly string _property;

        /// <summary>
        /// Initializes a new instance of the AdditionalNodeInfoAttribute class.
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
        public string Property
        {
            get { return _property; }
        }
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
            return new TreeNode(title + GetAdditionalInfo(nodeData)) {Tag = nodeData};
        }

        private static TreeNode[] InternalCreateNodes(IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach (var o in dictionary)
                result.Add(CreateNode(result.Count.ToString(), o));
            return result.ToArray();
        }

        private static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach (var o in list)
                result.Add(CreateNode(result.Count.ToString(), o));
            return result.ToArray();
        }

        private static TreeNode[] InternalCreateNodes(DictionaryEntry dictionaryEntry)
        {
            return new[]
                       {
                           CreateNode("key", dictionaryEntry.Key),
                           CreateNode("value", dictionaryEntry.Value)
                       };
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

        private static TreeNode[] InternalCreateNodes(object target)
        {
            var result = new List<TreeNode>();
            result.AddRange(CreateFieldNodes(target));
            result.AddRange(CreatePropertyNodes(target));
            return result.ToArray();
        }

        private static TreeNode[] CreateNodes(object target)
        {
            var xn = target as ITreeNodeSupport;
            if (xn != null)
                return xn.CreateNodes();


            var xl = target as IList;
            if (xl != null)
                return InternalCreateNodes(xl);
            var xd = target as IDictionary;
            if (xd != null)
                return InternalCreateNodes(xd);

            if (target is DictionaryEntry)
                return InternalCreateNodes((DictionaryEntry)target);

            return InternalCreateNodes(target);
        }

        private static TreeNode[] CreatePropertyNodes(object nodeData)
        {
            var result = new List<TreeNode>();
            foreach (var propertyInfo in nodeData.GetType().GetProperties(DefaultBindingFlags))
            {
                var treeNode = CreateTreeNode(nodeData, propertyInfo);
                if (treeNode != null)
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
                if (treeNode != null)
                    result.Add(treeNode);
            }
            return result.ToArray();
        }

        private static BindingFlags DefaultBindingFlags
        {
            get { return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; }
        }

        private delegate object GetObjectDelegate();

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
            var attrs = memberInfo.GetCustomAttributes(typeof (NodeAttribute), true);
            if (attrs.Length == 0)
                return null;

            var attr = (NodeAttribute) attrs[0];
            if (!attr.IsEnabled)
                return null;

            var value = getValue();
            if (value == null)
                return null;

            return CreateNode(attr.Title ?? memberInfo.Name, value);
        }

        private static void AddNodes(TreeNodeCollection nodes, object target)
        {
            nodes.Clear();
            nodes.AddRange(CreateNodes(target));
        }

        public static void Connect(TreeView treeView, object target)
        {
            AddNodes(treeView.Nodes, target);
            AddSubNodes(treeView.Nodes);
            treeView.BeforeExpand += treeView_BeforeExpand;
        }

        private static void AddSubNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
                AddNodes(node.Nodes, node.Tag);
        }

        private static void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            AddSubNodes(e.Node.Nodes);
        }
    }

    public interface ITreeNodeSupport {
        TreeNode[] CreateNodes();
    }
}