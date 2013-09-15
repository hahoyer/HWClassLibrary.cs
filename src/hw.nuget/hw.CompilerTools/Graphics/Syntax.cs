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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hw.Debug;

namespace hw.Graphics
{
    sealed class Syntax : DumpableObject
    {
        readonly string _name;
        readonly Syntax[] _children;
        readonly ISyntaxDrawer _drawer;

        Syntax(ISyntaxDrawer drawer, string name, IGraphTarget[] children)
        {
            _name = name;
            _drawer = drawer;
            _children = children.Select(target => Create(target, drawer)).ToArray();
        }

        bool HasChildren { get { return _children.Any(c => c != null); } }
        static int SaveWidth(Syntax syntax) { return syntax == null ? 0 : syntax.Width; }
        static int SaveHeight(Syntax syntax) { return syntax == null ? 0 : syntax.Height; }
        static int SaveAnchorWidth(Syntax syntax) { return syntax == null ? 0 : syntax.Anchor.Width; }

        internal int Height { get { return NodeHeight + (HasChildren ? _drawer.Gap.Height + ChildrenHeight : 0); } }
        internal int Width { get { return Math.Max(NodeOffset + NodeWidth, HasChildren ? ChildrenWidth : 0); } }

        int NodeHeight { get { return _drawer.NodeHeight(_name); } }
        int NodeWidth { get { return _drawer.NodeWidth(_name); } }

        Size Anchor { get { return new Size(AnchorOffset, NodeHeight / 2); } }
        int NodeOffset { get { return AnchorOffset - NodeWidth / 2; } }
        int ChildrenOffset { get { return Math.Max(0, (NodeWidth - ChildrenWidth) / 2); } }

        internal void Draw(Point origin)
        {
            if(HasChildren)
                DrawChildren(origin);
            DrawNode(origin);
        }

        void DrawNode(Point origin)
        {
            Tracer.Assert(NodeOffset >= 0, () => NodeOffset.ToString());
            _drawer.DrawNode(origin + new Size(NodeOffset, 0), _name);
        }

        void DrawChildren(Point origin)
        {
            var offsets = ChildOffsets.ToArray();
            for(var index = 0; index < _children.Length; index++)
                if(_children[index] != null)
                {
                    _drawer.DrawLine(origin + Anchor, origin + offsets[index] + _children[index].Anchor);
                    _children[index].Draw(origin + offsets[index]);
                }
        }

        IEnumerable<Size> ChildOffsets
        {
            get
            {
                Tracer.Assert(HasChildren);
                var height = NodeHeight + _drawer.Gap.Height;
                var currentWidthOffset = ChildrenOffset;
                foreach(var syntax in _children)
                {
                    yield return new Size(currentWidthOffset, height);
                    currentWidthOffset += SaveWidth(syntax) + _drawer.Gap.Width;
                }
                yield break;
            }
        }

        int ChildrenWidth
        {
            get
            {
                Tracer.Assert(HasChildren);
                var gapWidth = _drawer.Gap.Width * (_children.Length - 1);
                var effectiveChildrenWidth = _children.Select(SaveWidth).Sum();
                return gapWidth + effectiveChildrenWidth;
            }
        }

        int ChildrenHeight
        {
            get
            {
                Tracer.Assert(HasChildren);
                return _children.Select(SaveHeight).Max();
            }
        }


        int AnchorOffset
        {
            get
            {
                var result = 0;
                if(HasChildren)
                {
                    var childAnchors = _children.Select(SaveAnchorWidth);
                    result = ChildOffsets.Select((o, i) => o.Width + childAnchors.ElementAt(i)).Sum() / _children.Length;
                }
                return Math.Max(result, NodeWidth / 2);
            }
        }

        internal static Syntax Create(IGraphTarget syntax, ISyntaxDrawer syntaxDrawer) { return syntax == null ? null : new Syntax(syntaxDrawer, syntax.Title, syntax.Children); }
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