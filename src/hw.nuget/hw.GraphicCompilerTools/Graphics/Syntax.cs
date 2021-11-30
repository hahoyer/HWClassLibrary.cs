using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.DebugFormatter;
// ReSharper disable CheckNamespace

namespace hw.Graphics
{
    sealed class Syntax : DumpableObject
    {
        readonly Syntax[] Children;
        readonly ISyntaxDrawer Drawer;
        readonly string Name;

        Syntax(ISyntaxDrawer drawer, string name, IGraphTarget[] children)
        {
            Name = name;
            Drawer = drawer;
            Children = children.Select(target => Create(target, drawer)).ToArray();
        }

        internal int Height => NodeHeight + (HasChildren? Drawer.Gap.Height + ChildrenHeight : 0);
        internal int Width => Math.Max(NodeOffset + NodeWidth, HasChildren? ChildrenWidth : 0);

        bool HasChildren => Children.Any(c => c != null);

        int NodeHeight => Drawer.NodeHeight(Name);
        int NodeWidth => Drawer.NodeWidth(Name);

        Size Anchor => new Size(AnchorOffset, NodeHeight / 2);
        int NodeOffset => AnchorOffset - NodeWidth / 2;
        int ChildrenOffset => Math.Max(0, (NodeWidth - ChildrenWidth) / 2);

        IEnumerable<Size> ChildOffsets
        {
            get
            {
                HasChildren.Assert();
                var height = NodeHeight + Drawer.Gap.Height;
                var currentWidthOffset = ChildrenOffset;
                foreach(var syntax in Children)
                {
                    yield return new Size(currentWidthOffset, height);
                    currentWidthOffset += SaveWidth(syntax) + Drawer.Gap.Width;
                }
            }
        }

        int ChildrenWidth
        {
            get
            {
                HasChildren.Assert();
                var gapWidth = Drawer.Gap.Width * (Children.Length - 1);
                var effectiveChildrenWidth = Children.Select(SaveWidth).Sum();
                return gapWidth + effectiveChildrenWidth;
            }
        }

        int ChildrenHeight
        {
            get
            {
                HasChildren.Assert();
                return Children.Select(SaveHeight).Max();
            }
        }


        int AnchorOffset
        {
            get
            {
                var result = 0;
                if(HasChildren)
                {
                    var childAnchors = Children.Select(SaveAnchorWidth);
                    result = ChildOffsets.Select((o, i) => o.Width + childAnchors.ElementAt(i)).Sum() /
                             Children.Length;
                }

                return Math.Max(result, NodeWidth / 2);
            }
        }

        internal void Draw(Point origin)
        {
            if(HasChildren)
                DrawChildren(origin);
            DrawNode(origin);
        }

        internal static Syntax Create
            (IGraphTarget syntax, ISyntaxDrawer syntaxDrawer)
            => syntax == null? null : new Syntax(syntaxDrawer, syntax.Title, syntax.Children);

        static int SaveWidth(Syntax syntax) => syntax == null? 0 : syntax.Width;
        static int SaveHeight(Syntax syntax) => syntax == null? 0 : syntax.Height;
        static int SaveAnchorWidth(Syntax syntax) => syntax == null? 0 : syntax.Anchor.Width;

        void DrawNode(Point origin)
        {
            (NodeOffset >= 0).Assert(() => NodeOffset.ToString());
            Drawer.DrawNode(origin + new Size(NodeOffset, 0), Name);
        }

        void DrawChildren(Point origin)
        {
            var offsets = ChildOffsets.ToArray();
            for(var index = 0; index < Children.Length; index++)
                if(Children[index] != null)
                {
                    Drawer.DrawLine(origin + Anchor, origin + offsets[index] + Children[index].Anchor);
                    Children[index].Draw(origin + offsets[index]);
                }
        }
    }

    interface ISyntaxDrawer
    {
        Size Gap { get; }
        int NodeHeight(string nodeName);
        int NodeWidth(string name);
        void DrawNode(Point origin, string nodeName);
        void DrawLine(Point start, Point end);
    }
}