using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace HWClassLibrary.TreeStructure
{
    public static class Extender
    {
        /// <summary>
        /// Creates a treenode.with a given title from an object
        /// </summary>
        /// <param name="nodeData"></param>
        /// <param name="title"></param>
        /// <param name="iconKey"></param>
        /// <param name="isDefaultIcon"></param>
        /// <returns></returns>
        public static TreeNode CreateNode(this object nodeData, string title, string iconKey, bool isDefaultIcon)
        {
            var result = new TreeNode(title + nodeData.GetAdditionalInfo()) {Tag = nodeData};
            if(iconKey == null)
                iconKey = nodeData.GetIconKey();
            if(isDefaultIcon)
            {
                var defaultIcon = nodeData.GetIconKey();
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

        public static TreeNode CreateNode(this object nodeData)
        {
            return CreateNode(nodeData, "", null, false);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        public static TreeNode CreateTaggedNode(this object nodeData, string title, string iconKey)
        {
            return nodeData.CreateTaggedNode(title, iconKey, false);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        public static TreeNode CreateNamedNode(this object nodeData, string title, string iconKey)
        {
            return nodeData.CreateNamedNode(title, iconKey, false);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="isDefaultIcon">if set to <c>true</c> [is default icon].</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateNamedNode(this object nodeData, string title, string iconKey, bool isDefaultIcon)
        {
            return nodeData.CreateNode(title + " = ", iconKey, isDefaultIcon);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="iconKey">The icon key.</param>
        /// <param name="isDefaultIcon">if set to <c>true</c> [is default icon].</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateTaggedNode(this object nodeData, string title, string iconKey, bool isDefaultIcon)
        {
            return nodeData.CreateNode(title + ": ", iconKey, isDefaultIcon);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        public static TreeNode CreateTaggedNode(this object nodeData, string title)
        {
            return nodeData.CreateTaggedNode(title, null, false);
        }

        /// <summary>
        /// Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="nodeData">The node data. Can be <see cref="IIconKeyProvider"/></param>
        /// <returns></returns>
        public static TreeNode CreateNamedNode(this object nodeData, string title)
        {
            return nodeData.CreateNamedNode(title, null, false);
        }

        private static TreeNode[] InternalCreateNodes(this IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach(var element in dictionary)
                result.Add(element.CreateNumberedNode(result.Count, "ListItem"));
            return result.ToArray();
        }

        private static TreeNode CreateNumberedNode(this object nodeData, int i, string iconKey)
        {
            return nodeData.CreateNumberedNode(i, iconKey, false);
        }

        private static TreeNode CreateNumberedNode(this object nodeData, int i, string iconKey, bool isDefaultIcon)
        {
            return nodeData.CreateNode("[" + i + "] ", iconKey, isDefaultIcon);
        }

        private static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach(var o in list)
                result.Add(o.CreateNumberedNode(result.Count, "ListItem", true));
            return result.ToArray();
        }

        private static TreeNode[] InternalCreateNodes(this DictionaryEntry dictionaryEntry)
        {
            return new[]
                       {
                           dictionaryEntry.Key.CreateTaggedNode("key", "Key", true),
                           dictionaryEntry.Value.CreateTaggedNode("value")
                       };
        }

        /// <summary>
        /// Gets the name of the icon.
        /// </summary>
        /// <param name="nodeData">The node data.</param>
        /// <returns></returns>
        private static string GetIconKey(this object nodeData)
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

        [NotNull]
        internal static string GetAdditionalInfo([CanBeNull] this object nodeData)
        {
            if(nodeData == null)
                return "<null>";

            if(nodeData is IAdditionalNodeInfoProvider)
                return ((IAdditionalNodeInfoProvider) nodeData).AdditionalNodeInfo;
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

        private static TreeNode[] InternalCreateNodes(this object target)
        {
            var result = new List<TreeNode>();
            result.AddRange(target.CreateFieldNodes());
            result.AddRange(target.CreatePropertyNodes());
            return result.ToArray();
        }

        private static TreeNode[] CreateNodes(this object target)
        {
            if(target == null)
                return new TreeNode[0];

            var xn = target as ITreeNodeSupport;
            if(xn != null)
                return xn.CreateNodes();

            var xl = target as IList;
            if(xl != null)
                return xl.InternalCreateNodes();
            var xd = target as IDictionary;
            if(xd != null)
                return xd.InternalCreateNodes();

            if(target is DictionaryEntry)
                return ((DictionaryEntry) target).InternalCreateNodes();

            return target.InternalCreateNodes();
        }

        private static TreeNode[] CreatePropertyNodes(this object nodeData)
        {
            var result = new List<TreeNode>();
            foreach(var propertyInfo in nodeData.GetType().GetProperties(DefaultBindingFlags))
            {
                var treeNode = nodeData.CreateTreeNode(propertyInfo);
                if(treeNode != null)
                    result.Add(treeNode);
            }
            return result.ToArray();
        }

        private static TreeNode[] CreateFieldNodes(this object nodeData)
        {
            var result = new List<TreeNode>();
            var type = nodeData.GetType();
            foreach(var fieldInfo in type.GetFields(DefaultBindingFlags))
            {
                var treeNode = nodeData.CreateTreeNode(fieldInfo);
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

        private static TreeNode CreateTreeNode(this object nodeData, FieldInfo fieldInfo)
        {
            return fieldInfo.CreateTreeNode(() => Value(fieldInfo, nodeData));
        }

        private static TreeNode CreateTreeNode(this object nodeData, PropertyInfo propertyInfo)
        {
            return propertyInfo.CreateTreeNode(() => Value(propertyInfo, nodeData));
        }

        private static object Value(this FieldInfo fieldInfo, object nodeData)
        {
            return fieldInfo.GetValue(nodeData);
        }

        private static object Value(this PropertyInfo propertyInfo, object nodeData)
        {
            return propertyInfo.GetValue(nodeData, null);
        }

        private static TreeNode CreateTreeNode(this MemberInfo memberInfo, Func<object> getValue)
        {
            var attrs = memberInfo.GetCustomAttributes(typeof(NodeAttribute), true);
            if(attrs.Length == 0)
                return null;

            var attr = (NodeAttribute) attrs[0];
            var value = getValue.CatchedEval();
            if(value == null)
                return null;

            var result = CreateNamedNode(value, memberInfo.Name, attr.IconKey);

            var smartNode = (SmartNodeAttribute[]) memberInfo.GetCustomAttributes(typeof(SmartNodeAttribute), true);
            if(smartNode.Length == 0)
                return result;

            return SmartNodeAttribute.Process(result);
        }

        private static object CatchedEval(this Func<object> value)
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

        private static void CreateNodeList(this TreeNodeCollection nodes, object target)
        {
            nodes.Clear();
            nodes.AddRange(target.CreateNodes());
        }

        public static void Connect(this TreeView treeView, object target)
        {
            target.Connect(treeView);
        }

        public static void Connect(this object target, TreeView treeView)
        {
            treeView.Nodes.CreateNodeList(target);
            treeView.Nodes.AddSubNodesAsync();
            treeView.BeforeExpand += BeforeExpand;
        }

        private static void AddSubNodesAsync(this TreeNodeCollection nodes)
        {
            lock(nodes)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += ((sender, e) => AddSubNodes(nodes));
                backgroundWorker.RunWorkerAsync();
            }
        }

        private static void AddSubNodes(TreeNodeCollection nodes)
        {
            lock (nodes)
            {
                foreach(TreeNode node in nodes)
                    node.CreateNodeList();
            }
        }

        internal static void CreateNodeList(this TreeNode node)
        {
            node.Nodes.CreateNodeList(node.Tag);
        }

        private static void BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.AddSubNodesAsync();
        }
    }
}