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

namespace hw.Graphics.SVG
{
    sealed class SyntaxDrawer : DumpableObject, ISyntaxDrawer
    {
        readonly Syntax _syntax;
        readonly Root _root;
        readonly float _tick;
        readonly string _stroke;
        readonly string _fillColor;
        readonly string _fontFamily;
        readonly Font _font;
        readonly System.Drawing.Graphics _graphics;

        internal SyntaxDrawer(IGraphTarget target)
        {
            _fontFamily = "Lucida Console";
            _stroke = "Black";
            _tick = 1;
            _fillColor = "LightBlue";
            _font = new Font(FontFamily.Families.Single(f1 => f1.Name == _fontFamily), _tick * 10);
            _graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            _root = new Root {Svg = new SVG()};
            _syntax = Syntax.Create(target, this);
        }

        internal Root Draw()
        {
            if(_syntax == null)
                return _root;

            _syntax.Draw(new Point(SizeBase / 2, SizeBase / 2));

            _root.Svg.Width = _syntax.Width + SizeBase + "px";
            _root.Svg.Height = _syntax.Height + SizeBase + "px";

            return _root;
        }

        int SizeBase { get { return (int) (_tick * 10); } }
        Size ISyntaxDrawer.Gap { get { return new Size(SizeBase, SizeBase); } }
        int ISyntaxDrawer.NodeHeight(string nodeName) { return Math.Max(TextSize(nodeName).Height, SizeBase) + SizeBase; }
        int ISyntaxDrawer.NodeWidth(string nodeName) { return Math.Max(TextSize(nodeName).Width, SizeBase) + SizeBase; }

        void ISyntaxDrawer.DrawNode(Point origin, string nodeName)
        {
            var size = NodeSize(nodeName);
            var bodyWidth = size.Width - 2 * SizeBase;

            _root.Svg.Items.Add(new Path {PathData = (origin + new Size(SizeBase, 0)).CloseAndFormat(bodyWidth.HorizontalLine(), SizeBase.Arc(new Size(0, SizeBase * 2), false, true), (-bodyWidth).HorizontalLine(), SizeBase.Arc(new Size(0, -SizeBase * 2), false, true)), Fill = _fillColor, Stroke = _stroke, StrokeWidth = _tick});

            _root.Svg.Items.Add(CreateText(nodeName, origin + new Size(size.Width / 2, size.Height / 2)));
        }

        Content CreateText(string text, Point center)
        {
            var size = TextSize(text);

            var start = center - new Size(size.Width / 2, size.Height / 2) + new Size(0, size.Height * 4 / 5);

            return new Text {Data = text, Start = start, FontFamily = _font.FontFamily.Name, Size = _font.Size * 7 / 5, Fill = _stroke};
        }

        void ISyntaxDrawer.DrawLine(Point start, Point end) { _root.Svg.Items.Add(CreateLine(start, end)); }

        Content CreateLine(Point start, Point end) { return new Path {PathData = start.Format(new Size(end.X - start.X, end.Y - start.Y).LineTo()), Stroke = _stroke, StrokeWidth = _tick}; }

        Size TextSize(string nodeName)
        {
            var result = _graphics.MeasureString(nodeName, _font);
            return new Size((int) result.Width, (int) result.Height);
        }

        Size NodeSize(string nodeName)
        {
            var textSize = TextSize(nodeName);
            return new Size(Math.Max(textSize.Width, SizeBase) + SizeBase, SizeBase * 2);
        }
    }
}