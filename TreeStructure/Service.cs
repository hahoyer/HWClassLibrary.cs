using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace HWClassLibrary.TreeStructure
{
    public static class Service
    {
        private static TreeNode CreateNode(string title, string iconKey, bool isDefaultIcon, object nodeData)
        {
            var result = new TreeNode(title + GetAdditionalInfo(nodeData)) {Tag = nodeData};
            if(iconKey == null)
                iconKey = GetIconKey(nodeData);
            if(isDefaultIcon)
            {
                var defaultIcon = GetIconKey(nodeData);
                if(defaultIcon != null)
                    iconKey = defaultIcon;
            }
            if(iconKey != null)
            {
                result.ImageKey = iconKey;
                result.SelectedImageKey = iconKey;
            }
            return result;
        }

        /// <summary>
        /// Creates the node.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        public static TreeNode CreateTaggedNode(string title, string iconKey, object nodeData)
        {
            return CreateTaggedNode(title, iconKey, false, nodeData);
        }

        /// <summary>
        /// Creates the node.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        public static TreeNode CreateNamedNode(string title, string iconKey, object nodeData)
        {
            return CreateNamedNode(title, iconKey, false, nodeData);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="isDefaultIcon">if set to <c>true</c> [is default icon].</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateNamedNode(string title, string iconKey, bool isDefaultIcon, object nodeData)
        {
            return CreateNode(title + " = ", iconKey, isDefaultIcon, nodeData);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="isDefaultIcon">if set to <c>true</c> [is default icon].</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateTaggedNode(string title, string iconKey, bool isDefaultIcon, object nodeData)
        {
            return CreateNode(title + ": ", iconKey, isDefaultIcon, nodeData);
        }

        /// <summary>
        /// Creates the node.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        public static TreeNode CreateTaggedNode(string title, object nodeData)
        {
            return CreateTaggedNode(title, null, false, nodeData);
        }

        /// <summary>
        /// Creates the node.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        public static TreeNode CreateNamedNode(string title, object nodeData)
        {
            return CreateNamedNode(title, null, false, nodeData);
        }

        private static TreeNode[] InternalCreateNodes(IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach(var o in dictionary)
                result.Add(CreateNumberedNode(result.Count, "ListItem", o));
            return result.ToArray();
        }

        private static TreeNode CreateNumberedNode(int i, string iconKey, object nodeData)
        {
            return CreateNumberedNode(i, iconKey, false, nodeData);
        }

        private static TreeNode CreateNumberedNode(int i, string iconKey, bool isDefaultIcon, object nodeData)
        {
            return CreateNode("[" + i + "] ", iconKey, isDefaultIcon, nodeData);
        }

        private static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach(var o in list)
                result.Add(CreateNumberedNode(result.Count, "ListItem", true, o));
            return result.ToArray();
        }

        private static TreeNode[] InternalCreateNodes(DictionaryEntry dictionaryEntry)
        {
            return new[]
                       {
                           CreateTaggedNode("key", "Key", true, dictionaryEntry.Key),
                           CreateTaggedNode("value", dictionaryEntry.Value)
                       };
        }

        /// <summary>
        /// Gets the name of the icon.
        /// </summary>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        private static string GetIconKey(object nodeData)
        {
            if(nodeData == null)
                return null;
            var ip = nodeData as IIconKeyProvider;
            if(ip != null)
                return ip.IconKey;
            if(nodeData is string)
                return "String";
            if(nodeData is bool)
                return "Bool";
            if(nodeData.GetType().IsPrimitive)
                return "Number";
            if(nodeData is IDictionary)
                return "Dictionary";
            if(nodeData is IList)
                return "List";

            return null;
        }

        internal static string GetAdditionalInfo(object nodeData)
        {
            if(nodeData == null)
                return "<null>";

            var attrs = nodeData.GetType().GetCustomAttributes(typeof(AdditionalNodeInfoAttribute), true);
            if(attrs.Length > 0)
            {
                var attr = (AdditionalNodeInfoAttribute) attrs[0];
                return nodeData.GetType().GetProperty(attr.Property).GetValue(nodeData, null).ToString();
            }

            var il = nodeData as IList;
            if(il != null)
            {
                var result = "IList";
                if(il.GetType().IsGenericType)
                    result += "<" + il.GetType().GetGenericArguments()[0].FullName + ">";
                return result + "[" + ((IList) nodeData).Count + "]";
            }

            if(nodeData.GetType().Namespace.StartsWith("System"))
                return nodeData.ToString();

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
            if(target == null)
                return new TreeNode[0];

            var xn = target as ITreeNodeSupport;
            if(xn != null)
                return xn.CreateNodes();

            var xl = target as IList;
            if(xl != null)
                return InternalCreateNodes(xl);
            var xd = target as IDictionary;
            if(xd != null)
                return InternalCreateNodes(xd);

            if(target is DictionaryEntry)
                return InternalCreateNodes((DictionaryEntry) target);

            return InternalCreateNodes(target);
        }

        private static TreeNode[] CreatePropertyNodes(object nodeData)
        {
            var result = new List<TreeNode>();
            foreach(var propertyInfo in nodeData.GetType().GetProperties(DefaultBindingFlags))
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
            foreach(var fieldInfo in type.GetFields(DefaultBindingFlags))
            {
                var treeNode = CreateTreeNode(nodeData, fieldInfo);
                if(treeNode != null)
                    result.Add(treeNode);
            }
            return result.ToArray();
        }

        private static BindingFlags DefaultBindingFlags
        {
            get
            {
                return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                       BindingFlags.FlattenHierarchy;
            }
        }

        private static TreeNode CreateTreeNode(object nodeData, FieldInfo fieldInfo)
        {
            return CreateTreeNode(fieldInfo, () => Value(fieldInfo, nodeData));
        }

        private static TreeNode CreateTreeNode(object nodeData, PropertyInfo propertyInfo)
        {
            return CreateTreeNode(propertyInfo, () => Value(propertyInfo, nodeData));
        }

        private static object Value(FieldInfo fieldInfo, object nodeData)
        {
            return fieldInfo.GetValue(nodeData);
        }

        private static object Value(PropertyInfo propertyInfo, object nodeData)
        {
            return propertyInfo.GetValue(nodeData, null);
        }

        private static TreeNode CreateTreeNode(MemberInfo memberInfo, Func<object> getValue)
        {
            var attrs = memberInfo.GetCustomAttributes(typeof(NodeAttribute), true);
            if(attrs.Length == 0)
                return null;

            var attr = (NodeAttribute) attrs[0];
            var value = CatchedEval(getValue);
            if(value == null)
                return null;

            var result = CreateNamedNode(memberInfo.Name, attr.IconKey, value);

            var smartNode = (SmartNodeAttribute[]) memberInfo.GetCustomAttributes(typeof(SmartNodeAttribute), true);
            if(smartNode.Length == 0)
                return result;

            return SmartNodeAttribute.Process(result);
        }

        private static object CatchedEval(Func<object> value)
        {
            try
            {
                return value();
            }
            catch(Exception e)
            {
                return e;
            }
        }

        private static void CreateNodeList(TreeNodeCollection nodes, object target)
        {
            nodes.Clear();
            nodes.AddRange(CreateNodes(target));
        }

        public static void Connect(TreeView treeView, object target)
        {
            CreateNodeList(treeView.Nodes, target);
            AddSubNodes(treeView.Nodes);
            treeView.BeforeExpand += BeforeExpand;
        }

        private static void AddSubNodes(TreeNodeCollection nodes)
        {
            foreach(TreeNode node in nodes)
                CreateNodeList(node);
        }

        internal static void CreateNodeList(TreeNode node)
        {
            CreateNodeList(node.Nodes, node.Tag);
        }

        private static void BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            AddSubNodes(e.Node.Nodes);
        }
    }
}