using System;
using System.Collections;
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
        private readonly string _title;
        private readonly bool _isEnabled = true;

        /// <summary>
        /// Default attribute to define a subnode for treeview. 
        /// Property name will be used as title of subnode
        /// </summary>
        /// created 06.02.2007 23:35
        public NodeAttribute() { }

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
    public class AdditionalNodeInfoAttribute: Attribute
    {
        private readonly string _property;

        /// <summary>
        /// Property to obtain additional node info
        /// </summary>
        public string Property { get { return _property; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AdditionalNodeInfoAttribute"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// created 07.02.2007 00:47
        public AdditionalNodeInfoAttribute(string property)
        {
            _property = property;
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
        public static TreeNode CreateNode(string title, object nodeData)
        {
            TreeNode result = new TreeNode(title);
            result.Tag = nodeData;
            return result;
        }

        public static void AddNodes(TreeNode target)
        {
            AddNodes(target,0);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object. Subnodes are appended until depth given
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data.</param>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        /// created 06.02.2007 23:27
        public static TreeNode CreateNode(string title, object nodeData, int depth)
        {
            if (nodeData == null)
                return new TreeNode(title + " = null");

            TreeNode result = InternalCreateNode(title, nodeData, depth);
            if (depth > 0 && result.Nodes.Count == 0 && title == result.Text)
                result.Text = result.Text + " [" + nodeData.GetType().FullName + "]";
            return result;
        }

        private static TreeNode InternalCreateNode(string title, object nodeData, int depth)
        {
            IList xl = nodeData as IList;
            if (xl != null)
                return InternalCreateNode(title, xl, depth);

            IDictionary xd = nodeData as IDictionary;
            if (xd != null)
                return InternalCreateNodex(title, xd, depth);

            string additionalInfo = GetAdditionalInfo(nodeData);
            TreeNode result = CreateNode(title + additionalInfo, nodeData);
            AddNodes(result,depth-1);
            return result;
        }

        private static TreeNode InternalCreateNodex(string title, IDictionary nodeData, int depth)
        {
            TreeNode result = CreateNode(title + " Count = " + nodeData.Count, nodeData);
            int i = 0;
            foreach (DictionaryEntry entry in nodeData)
            {
                TreeNode treeNode = CreateNode(i.ToString() + " " + entry.Key.ToString(), entry);
                treeNode.Nodes.Add(CreateNode("key", entry.Key, depth-1));
                treeNode.Nodes.Add(CreateNode("value", entry.Value, depth - 1));
                result.Nodes.Add(treeNode);
                i++;
            }
            return result;
        }

        private static TreeNode InternalCreateNode(string title, IList nodeData, int depth)
        {
            TreeNode result = CreateNode(title + " Count = "+ nodeData.Count, nodeData);
            for (int i = 0; i < nodeData.Count; i++)
            {
                TreeNode treeNode = CreateNode(i.ToString(), nodeData[i], depth-1);
                result.Nodes.Add(treeNode);
            }
            return result;
        }

        private static string GetAdditionalInfo(object nodeData)
        {
            object[] attrs = nodeData.GetType().GetCustomAttributes(typeof (AdditionalNodeInfoAttribute), true);
            if (attrs.Length > 0)
            {
                AdditionalNodeInfoAttribute attr = (AdditionalNodeInfoAttribute) attrs[0];
                return " " + nodeData.GetType().GetProperty(attr.Property).GetValue(nodeData, null);
            }

            if (nodeData.GetType().Namespace.StartsWith("System"))
                return " = " + nodeData;

            return "";
        }

        private static void AddNodes(TreeNode target, int depth)
        {
            if(depth < 0)
                return;

            object nodeData = target.Tag;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (FieldInfo fieldInfo in nodeData.GetType().GetFields(flags))
            {
                object[] attrs = fieldInfo.GetCustomAttributes(typeof(NodeAttribute),true);
                if (attrs.Length > 0)
                {
                    NodeAttribute attr = (NodeAttribute) attrs[0];
                    if (attr.IsEnabled)
                    {
                        string title = attr.Title; 
                        if(title == null)
                            title = fieldInfo.Name;

                        object value = fieldInfo.GetValue(nodeData);
                        if(value != null)
                            target.Nodes.Add(CreateNode(title, value, depth));
                    }
                    
                }
            }
            foreach (PropertyInfo propertyInfo in nodeData.GetType().GetProperties(flags))
            {
                object[] attrs = propertyInfo.GetCustomAttributes(typeof(NodeAttribute), true);
                if (attrs.Length > 0)
                {
                    NodeAttribute attr = (NodeAttribute)attrs[0];
                    if (attr.IsEnabled)
                    {
                        string title = attr.Title;
                        if (title == null)
                            title = propertyInfo.Name;

                        object value = propertyInfo.GetValue(nodeData, null);
                        if (value != null)
                            target.Nodes.Add(CreateNode(title, value, depth));
                    }

                }
            }
            ;
        }
    }
}
