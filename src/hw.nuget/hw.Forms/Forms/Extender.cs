#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.Forms
{
    public static class Extension
    {
        /// <summary>
        ///     Creates a treenode.with a given title from an object
        /// </summary>
        /// <param name="nodeData"> </param>
        /// <param name="title"> </param>
        /// <param name="iconKey"> </param>
        /// <param name="isDefaultIcon"> </param>
        /// <returns> </returns>
        public static TreeNode CreateNode(this object nodeData, string title = "", string iconKey = null, bool isDefaultIcon = false)
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

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        public static TreeNode CreateTaggedNode(this object nodeData, string title, string iconKey) { return nodeData.CreateTaggedNode(title, iconKey, false); }

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        public static TreeNode CreateNamedNode(this object nodeData, string title, string iconKey) { return nodeData.CreateNamedNode(title, iconKey, false); }

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="isDefaultIcon"> if set to <c>true</c> [is default icon]. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateNamedNode(this object nodeData, string title, string iconKey, bool isDefaultIcon) { return nodeData.CreateNode(title + " = ", iconKey, isDefaultIcon); }

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="isDefaultIcon"> if set to <c>true</c> [is default icon]. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateTaggedNode(this object nodeData, string title, string iconKey, bool isDefaultIcon) { return nodeData.CreateNode(title + ": ", iconKey, isDefaultIcon); }

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        public static TreeNode CreateTaggedNode(this object nodeData, string title) { return nodeData.CreateTaggedNode(title, null, false); }

        /// <summary>
        ///     Creates a treenode.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <returns> </returns>
        public static TreeNode CreateNamedNode(this object nodeData, string title) { return nodeData.CreateNamedNode(title, null, false); }

        static TreeNode[] InternalCreateNodes(IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach(var element in dictionary)
                result.Add(CreateNumberedNode(element, result.Count, "ListItem"));
            return result.ToArray();
        }

        static TreeNode CreateNumberedNode(object nodeData, int i, string iconKey, bool isDefaultIcon = false) { return nodeData.CreateNode("[" + i + "] ", iconKey, isDefaultIcon); }

        static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach(var o in list)
                result.Add(CreateNumberedNode(o, result.Count, "ListItem", true));
            return result.ToArray();
        }

        static TreeNode[] InternalCreateNodes(DictionaryEntry dictionaryEntry) { return new[] {dictionaryEntry.Key.CreateTaggedNode("key", "Key", true), dictionaryEntry.Value.CreateTaggedNode("value")}; }

        /// <summary>
        ///     Gets the name of the icon.
        /// </summary>
        /// <param name="nodeData"> The node data. </param>
        /// <returns> </returns>
        static string GetIconKey(this object nodeData)
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

            var additionalNodeInfoProvider = nodeData as IAdditionalNodeInfoProvider;
            if(additionalNodeInfoProvider != null)
                return additionalNodeInfoProvider.AdditionalNodeInfo;
            var attrs = nodeData.GetType().GetCustomAttributes(typeof(AdditionalNodeInfoAttribute), true);
            if(attrs.Length > 0)
            {
                var attr = (AdditionalNodeInfoAttribute) attrs[0];
                return nodeData.GetType().GetProperty(attr.Property).GetValue(nodeData, null).ToString();
            }

            var il = nodeData as IList;
            if(il != null)
                return il.GetType().PrettyName() + "[" + ((IList) nodeData).Count + "]";

            var nameSpace = nodeData.GetType().Namespace;
            if(nameSpace != null && nameSpace.StartsWith("System"))
                return nodeData.ToString();

            return "";
        }

        static TreeNode[] InternalCreateNodes(object target)
        {
            var result = new List<TreeNode>();
            result.AddRange(CreateFieldNodes(target));
            result.AddRange(CreatePropertyNodes(target));
            return result.ToArray();
        }

        public static TreeNode[] CreateNodes(this object target)
        {
            if(target == null)
                return new TreeNode[0];

            var xn = target as ITreeNodeSupport;
            if(xn != null)
                return xn.CreateNodes().ToArray();

            return CreateAutomaticNodes(target);
        }

        public static TreeNode[] CreateAutomaticNodes(this object target)
        {
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

        static TreeNode[] CreatePropertyNodes(object nodeData) { return nodeData.GetType().GetProperties(DefaultBindingFlags).Select(propertyInfo => CreateTreeNode(nodeData, propertyInfo)).Where(treeNode => treeNode != null).ToArray(); }

        static TreeNode[] CreateFieldNodes(object nodeData) { return nodeData.GetType().GetFieldInfos().Select(fieldInfo => CreateTreeNode(nodeData, fieldInfo)).Where(treeNode => treeNode != null).ToArray(); }

        static BindingFlags DefaultBindingFlags { get { return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy; } }

        static TreeNode CreateTreeNode(object nodeData, FieldInfo fieldInfo) { return CreateTreeNode(fieldInfo, () => Value(fieldInfo, nodeData)); }

        static TreeNode CreateTreeNode(object nodeData, PropertyInfo propertyInfo) { return CreateTreeNode(propertyInfo, () => Value(propertyInfo, nodeData)); }

        static object Value(FieldInfo fieldInfo, object nodeData) { return fieldInfo.GetValue(nodeData); }

        static object Value(PropertyInfo propertyInfo, object nodeData) { return propertyInfo.GetValue(nodeData, null); }

        static TreeNode CreateTreeNode(MemberInfo memberInfo, Func<object> getValue)
        {
            var attribute = memberInfo.GetAttribute<NodeAttribute>(true);
            if(attribute == null)
                return null;

            var value = CatchedEval(getValue);
            if(value == null)
                return null;

            var result = CreateNamedNode(value, memberInfo.Name, attribute.IconKey);

            if(memberInfo.GetAttribute<SmartNodeAttribute>(true) == null)
                return result;

            return SmartNodeAttribute.Process(result);
        }

        static object CatchedEval(Func<object> value)
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

        static void CreateNodeList(TreeNodeCollection nodes, object target)
        {
            var treeNodes = CreateNodes(target);
            //Tracer.FlaggedLine(treeNodes.Dump());
            //Tracer.ConditionalBreak(treeNodes.Length == 20,"");
            nodes.Clear();
            nodes.AddRange(treeNodes);
        }

        public static void Connect(this TreeView treeView, object target) { Connect(target, treeView); }

        public static void Connect(this object target, TreeView treeView)
        {
            CreateNodeList(treeView.Nodes, target);
            AddSubNodes(treeView.Nodes);
            treeView.BeforeExpand += BeforeExpand;
        }

        static void AddSubNodesAsync(TreeNodeCollection nodes)
        {
            lock(nodes)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += ((sender, e) => AddSubNodes(nodes));
                backgroundWorker.RunWorkerAsync();
            }
        }

        static void AddSubNodes(TreeNodeCollection nodes)
        {
            foreach(TreeNode node in nodes)
                CreateNodeList(node);
        }

        internal static void CreateNodeList(this TreeNode node) { CreateNodeList(node.Nodes, node.Tag); }

        static void BeforeExpand(object sender, TreeViewCancelEventArgs e) { AddSubNodes(e.Node.Nodes); }
    }
}