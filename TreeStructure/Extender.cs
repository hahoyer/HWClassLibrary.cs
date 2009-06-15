using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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

        private static TreeNode[] InternalCreateNodes(IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach(var element in dictionary)
                result.Add(CreateNumberedNode(element, result.Count, "ListItem"));
            return result.ToArray();
        }

        private static TreeNode CreateNumberedNode(object nodeData, int i, string iconKey)
        {
            return CreateNumberedNode(nodeData, i, iconKey, false);
        }

        private static TreeNode CreateNumberedNode(object nodeData, int i, string iconKey, bool isDefaultIcon)
        {
            return nodeData.CreateNode("[" + i + "] ", iconKey, isDefaultIcon);
        }

        private static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach(var o in list)
                result.Add(CreateNumberedNode(o, result.Count, "ListItem", true));
            return result.ToArray();
        }

        private static TreeNode[] InternalCreateNodes(DictionaryEntry dictionaryEntry)
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
                var treeNode = CreateTreeNode(nodeData,fieldInfo);
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
            return CreateTreeNode(propertyInfo,() => Value(propertyInfo, nodeData));
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

            var result = CreateNamedNode(value, memberInfo.Name, attr.IconKey);

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
            var treeNodes = CreateNodes(target);
            Tracer.FlaggedLine(treeNodes.Dump());
            //Tracer.ConditionalBreak(treeNodes.Length == 20,"");
            nodes.Clear();
            nodes.AddRange(treeNodes);
        }

        public static void Connect(this TreeView treeView, object target)
        {
            Connect(target,treeView);
        }

        public static void Connect(this object target, TreeView treeView)
        {
            CreateNodeList(treeView.Nodes,target);
            AddSubNodes(treeView.Nodes);
            treeView.BeforeExpand += BeforeExpand;
        }

        private static void AddSubNodesAsync(TreeNodeCollection nodes)
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
            foreach(TreeNode node in nodes)
                CreateNodeList(node);
        }

        internal static void CreateNodeList(this TreeNode node)
        {
            CreateNodeList(node.Nodes, node.Tag);
        }

        private static void BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            AddSubNodes(e.Node.Nodes);
        }
    }
}