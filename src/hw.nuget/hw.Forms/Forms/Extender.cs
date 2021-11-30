using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Forms
{
    [PublicAPI]
    public static class Extension
    {
        public const int ShiftKey = 4;
        public const int AltKey = 32;
        public const int ControlKey = 8;
        public const int LeftMouseButton = 1;
        public const int RightMouseButton = 2;
        public const int MiddleMouseButton = 16;

        static BindingFlags DefaultBindingFlags => BindingFlags.Public
                                                   | BindingFlags.NonPublic
                                                   | BindingFlags.Instance
                                                   | BindingFlags.FlattenHierarchy;

        /// <summary>
        ///     Creates a tree-node.with a given title from an object
        /// </summary>
        /// <param name="nodeData"> </param>
        /// <param name="title"> </param>
        /// <param name="iconKey"> </param>
        /// <param name="isDefaultIcon"> </param>
        /// <param name="name"></param>
        /// <returns> </returns>
        public static TreeNode CreateNode
        (
            this object nodeData,
            string title = "",
            string iconKey = null,
            bool isDefaultIcon = false,
            string name = null
        )
        {
            var text = title + nodeData.GetAdditionalInfo();
            var effectiveName = nodeData.GetName(name) ?? text;
            var result = new TreeNode(text) {Tag = nodeData, Name = effectiveName};
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
        ///     Creates a tree-node.with a given title from an object (format: &lt;title&gt; = &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="isDefaultIcon"> if set to <c>true</c> [is default icon]. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <param name="name"></param>
        /// <returns> </returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateNamedNode
        (
            this object nodeData,
            string title,
            string iconKey = null,
            bool isDefaultIcon = false,
            string name = null
        )
            => nodeData
                .CreateNode(title + " = ", iconKey, isDefaultIcon, name);

        /// <summary>
        ///     Creates a tree-node.with a given title from an object (format: &lt;title&gt;: &lt;nodeData.ToString()&gt;)
        /// </summary>
        /// <param name="title"> The title. </param>
        /// <param name="iconKey"> The icon key. </param>
        /// <param name="isDefaultIcon"> if set to <c>true</c> [is default icon]. </param>
        /// <param name="nodeData"> The node data. Can be <see cref="IIconKeyProvider" /> </param>
        /// <param name="name"></param>
        /// <returns> </returns>
        /// created 06.02.2007 23:26
        public static TreeNode CreateTaggedNode
        (
            this object nodeData,
            string title,
            string iconKey = null,
            bool isDefaultIcon = false,
            string name = null
        )
            => nodeData
                .CreateNode(title + ": ", iconKey, isDefaultIcon, name);

        /// <summary>
        ///     Gets the name of the icon.
        /// </summary>
        /// <param name="nodeData"> The node data. </param>
        /// <returns> </returns>
        public static string GetIconKey(this object nodeData)
        {
            if(nodeData == null)
                return null;
            if(nodeData is IIconKeyProvider ip)
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

        public static TreeNode[] CreateNodes(this object target)
        {
            if(target == null)
                return new TreeNode[0];

            if(target is ITreeNodeSupport xn)
                return xn.CreateNodes().ToArray();

            return CreateAutomaticNodes(target);
        }

        public static TreeNode[] CreateAutomaticNodes(this object target)
        {
            if(target is IList xl)
                return InternalCreateNodes(xl);
            if(target is IDictionary xd)
                return InternalCreateNodes(xd);

            if(target is DictionaryEntry entry)
                return InternalCreateNodes(entry);

            return InternalCreateNodes(target);
        }

        public static void Connect(this TreeView treeView, object target) => Connect(target, treeView);

        public static void Connect(this object target, TreeView treeView)
        {
            CreateNodeList(treeView.Nodes, target);
            AddSubNodes(treeView.Nodes);
            treeView.BeforeExpand += (sender, e) => BeforeExpand(e.Node.Nodes);
        }

        public static void BeforeExpand(this TreeNodeCollection nodes)
        {
            LazyNodes(nodes);
            AddSubNodes(nodes);
        }

        /// <summary>
        ///     Installs a <see cref="PositionConfig" /> for target
        /// </summary>
        /// <param name="target">the form that will be watched</param>
        /// <param name="getFileName">
        ///     function to obtain filename of configuration file.
        ///     <para>It will be called each time the name is required. </para>
        ///     <para>Default: Target.Name</para>
        /// </param>
        public static PositionConfig InstallPositionConfig
            (this Form target, Func<string> getFileName = null) => new PositionConfig(getFileName) {Target = target};

        /// <summary>
        ///     Turns collection into IEnumerable
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<TreeNode> _(this TreeNodeCollection value) => value.Cast<TreeNode>();

        /// <summary>
        ///     Turns collection into IEnumerable
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<Control> _(this Control.ControlCollection value) => value.Cast<Control>();

        /// <summary>
        ///     Flatten node hierarchy
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        // ReSharper disable once IdentifierTypo
        public static IEnumerable<TreeNode> SelectHierachical(this TreeView tree) => tree
            .Nodes
            ._()
            .SelectMany(n => n.SelectHierarchical(nn => nn.Nodes._()));

        /// <summary>
        ///     Call a function or invoke it if required
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static T ThreadCallGuard<T>(this Control control, Func<T> function)
        {
            if(control.InvokeRequired)
                return (T)control.Invoke(function);
            return function();
        }

        /// <summary>
        ///     Call an action or invoke it if required
        /// </summary>
        /// <param name="control"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static void ThreadCallGuard(this Control control, Action function)
        {
            if(control.InvokeRequired)
                control.Invoke(function);
            else
                function();
        }

        public static bool SetEffect<T>(this DragEventArgs e, Func<T, bool> getIsValid, DragDropEffects defaultEffect)
        {
            e.Effect = ObtainEffect(e, getIsValid, defaultEffect);
            return e.Effect != DragDropEffects.None;
        }

        public static DragDropEffects ObtainEffect<T>
            (DragEventArgs e, Func<T, bool> getIsValid, DragDropEffects defaultEffect)
        {
            if(e.IsValid(getIsValid))
            {
                if(e.KeyState.HasBitSet(AltKey) && e.AllowedEffect.HasFlag(DragDropEffects.Link))
                    return DragDropEffects.Link;
                if(e.KeyState.HasBitSet(ShiftKey) && e.AllowedEffect.HasFlag(DragDropEffects.Move))
                    return DragDropEffects.Move;
                if(e.KeyState.HasBitSet(ControlKey) && e.AllowedEffect.HasFlag(DragDropEffects.Copy))
                    return DragDropEffects.Copy;
                if(e.AllowedEffect.HasFlag(defaultEffect))
                    return defaultEffect;
            }

            return DragDropEffects.None;
        }

        public static bool IsValid<T>
            (this DragEventArgs e, Func<T, bool> getIsValid) => e.Data.GetDataPresent(typeof(T))
                                                                && getIsValid((T)e.Data.GetData(typeof(T)));

        public static bool SetEffectCopy(this DragEventArgs e, bool isValid)
        {
            (e.AllowedEffect == DragDropEffects.Copy).Assert();

            e.Effect = isValid && e.KeyState.HasBitSet(ControlKey)? DragDropEffects.Copy : DragDropEffects.None;
            return e.Effect != DragDropEffects.None;
        }

        public static Bitmap AsBitmap(this Control c)
        {
            var result = new Bitmap(c.Width, c.Height);
            c.DrawToBitmap(result, new Rectangle(0, 0, c.Width, c.Height));
            return result;
        }

        public static Cursor ToCursor(this Control control, Point? hotSpot = null)
        {
            var iconInfo = new CursorUtil.IconInfo(control.AsBitmap());
            if(hotSpot != null)
                iconInfo.HotSpot = hotSpot.Value;
            return iconInfo.Cursor;
        }

        internal static string GetName([CanBeNull] this object nodeData, string name)
        {
            if(nodeData == null)
                return name;

            if(nodeData is INodeNameProvider additionalNodeInfoProvider)
                return additionalNodeInfoProvider.Value(name);
            var attr = nodeData.GetType().GetAttribute<NodeNameAttribute>(true);
            if(attr != null)
                return nodeData.GetType()
                    .InvokeMember(attr.Property, BindingFlags.Default, null, nodeData, new object[] {name}).ToString();

            return name;
        }

        [NotNull]
        internal static string GetAdditionalInfo([CanBeNull] this object nodeData)
        {
            if(nodeData == null)
                return "<null>";

            if(nodeData is IAdditionalNodeInfoProvider additionalNodeInfoProvider)
                return additionalNodeInfoProvider.AdditionalNodeInfo;
            var attr = nodeData.GetType().GetAttribute<AdditionalNodeInfoAttribute>(true);
            if(attr != null)
                return nodeData.GetType().GetProperty(attr.Property)?.GetValue(nodeData, null).ToString() ?? "";

            if(nodeData is IList il)
                return il.GetType().PrettyName() + "[" + il.Count + "]";

            var nameSpace = nodeData.GetType().Namespace;
            if(nameSpace != null && nameSpace.StartsWith("System"))
                return nodeData.ToString();

            return "";
        }

        internal static void CreateNodeList(this TreeNode node) => CreateNodeList(node.Nodes, node.Tag);

        internal static void CreateLazyNodeList(this TreeNode node)
        {
            var probe = node.Tag as ITreeNodeProbeSupport;
            if(probe == null || probe.IsEmpty)
            {
                CreateNodeList(node);
                return;
            }

            node.Nodes.Clear();
            // ReSharper disable once StringLiteralTypo
            node.Nodes.Add(new TreeNode {Tag = new LazyNode {Target = node.Tag}, Text = "<lazynode>"});
        }

        static TreeNode[] InternalCreateNodes(IDictionary dictionary)
        {
            var result = new List<TreeNode>();
            foreach(var element in dictionary)
                result.Add(CreateNumberedNode(element, result.Count, "ListItem"));
            return result.ToArray();
        }

        static TreeNode CreateNumberedNode
        (
            object nodeData,
            int i,
            string iconKey,
            bool isDefaultIcon = false,
            string name = null
        )
            => nodeData
                .CreateNode("[" + i + "] ", iconKey, isDefaultIcon, name ?? i.ToString());

        static TreeNode[] InternalCreateNodes(IList list)
        {
            var result = new List<TreeNode>();
            foreach(var o in list)
                result.Add(CreateNumberedNode(o, result.Count, "ListItem", true));
            return result.ToArray();
        }

        static TreeNode[] InternalCreateNodes(DictionaryEntry dictionaryEntry) => new[]
        {
            dictionaryEntry.Key.CreateTaggedNode("key", "Key", true), dictionaryEntry.Value.CreateTaggedNode("value")
        };

        static TreeNode[] InternalCreateNodes(object target)
        {
            var result = new List<TreeNode>();
            result.AddRange(CreateFieldNodes(target));
            result.AddRange(CreatePropertyNodes(target));
            return result.ToArray();
        }

        static TreeNode[] CreatePropertyNodes(object nodeData) => nodeData
            .GetType()
            .GetProperties(DefaultBindingFlags)
            .Select(propertyInfo => CreateTreeNode(nodeData, propertyInfo))
            .Where(treeNode => treeNode != null)
            .ToArray();

        static TreeNode[] CreateFieldNodes(object nodeData) => nodeData
            .GetType()
            .GetFieldInfos()
            .Select(fieldInfo => CreateTreeNode(nodeData, fieldInfo))
            .Where(treeNode => treeNode != null)
            .ToArray();

        static TreeNode CreateTreeNode(object nodeData, FieldInfo fieldInfo) => CreateTreeNode(
            fieldInfo,
            () => Value(fieldInfo, nodeData));

        static TreeNode CreateTreeNode(object nodeData, PropertyInfo propertyInfo) => CreateTreeNode(
            propertyInfo,
            () => Value(propertyInfo, nodeData));

        static object Value(FieldInfo fieldInfo, object nodeData) => fieldInfo.GetValue(nodeData);

        static object Value(PropertyInfo propertyInfo, object nodeData) => propertyInfo.GetValue(nodeData, null);

        static TreeNode CreateTreeNode(MemberInfo memberInfo, Func<object> getValue)
        {
            var attribute = memberInfo.GetAttribute<NodeAttribute>(true);
            if(attribute == null)
                return null;

            var value = CatchedEval(getValue);
            if(value == null)
                return null;

            var result = CreateNamedNode(value, memberInfo.Name, attribute.IconKey, name: attribute.Name);

            return memberInfo.GetAttribute<SmartNodeAttribute>(true) == null? result : SmartNodeAttribute.Process(result);
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

        static void AddSubNodesAsync(TreeNodeCollection nodes)
        {
            lock(nodes)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (sender, e) => AddSubNodes(nodes);
                backgroundWorker.RunWorkerAsync();
            }
        }

        static void AddSubNodes(TreeNodeCollection nodes)
        {
            foreach(TreeNode node in nodes)
                CreateLazyNodeList(node);
        }

        static void LazyNodes(TreeNodeCollection nodes)
        {
            if(nodes.Count != 1)
                return;
            var lazyNode = nodes[0].Tag as LazyNode;
            if(lazyNode == null)
                return;
            CreateNodeList(nodes, lazyNode.Target);
        }
    }
}