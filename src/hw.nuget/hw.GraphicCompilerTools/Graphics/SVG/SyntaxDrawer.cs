using System;
using System.Drawing;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Graphics.SVG
{
    sealed class SyntaxDrawer
        : DumpableObject
            , ISyntaxDrawer
    {
        readonly string FillColor;
        readonly Font Font;
        readonly System.Drawing.Graphics Graphics;
        readonly Root Root;
        readonly string Stroke;
        readonly Syntax Syntax;
        readonly float Tick;

        internal SyntaxDrawer(IGraphTarget target)
        {
            var fontFamily = "Lucida Console";
            Stroke = "Black";
            Tick = 1;
            FillColor = "LightBlue";
            Font = new Font(FontFamily.Families.Single(f1 => f1.Name == fontFamily), Tick * 10);
            Graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            Root = new Root {Svg = new SVG()};
            Syntax = Syntax.Create(target, this);
        }

        int SizeBase => (int)(Tick * 10);

        void ISyntaxDrawer.DrawLine(Point start, Point end) => Root.Svg.Items.Add(CreateLine(start, end));

        void ISyntaxDrawer.DrawNode(Point origin, string nodeName)
        {
            var size = NodeSize(nodeName);
            var bodyWidth = size.Width - 2 * SizeBase;

            Root.Svg.Items.Add(new Path
            {
                PathData = (origin + new Size(SizeBase, 0)).CloseAndFormat(bodyWidth.HorizontalLine()
                    , SizeBase.Arc(new Size(0, SizeBase * 2), false, true), (-bodyWidth).HorizontalLine()
                    , SizeBase.Arc(new Size(0, -SizeBase * 2), false, true))
                , Fill = FillColor, Stroke = Stroke, StrokeWidth = Tick
            });

            Root.Svg.Items.Add(CreateText(nodeName, origin + new Size(size.Width / 2, size.Height / 2)));
        }

        Size ISyntaxDrawer.Gap => new Size(SizeBase, SizeBase);
        int ISyntaxDrawer.NodeHeight(string nodeName) => Math.Max(TextSize(nodeName).Height, SizeBase) + SizeBase;
        int ISyntaxDrawer.NodeWidth(string nodeName) => Math.Max(TextSize(nodeName).Width, SizeBase) + SizeBase;

        internal Root Draw()
        {
            if(Syntax == null)
                return Root;

            Syntax.Draw(new Point(SizeBase / 2, SizeBase / 2));

            Root.Svg.Width = Syntax.Width + SizeBase + "px";
            Root.Svg.Height = Syntax.Height + SizeBase + "px";

            return Root;
        }

        Content CreateText(string text, Point center)
        {
            var size = TextSize(text);

            var start = center - new Size(size.Width / 2, size.Height / 2) + new Size(0, size.Height * 4 / 5);

            return new Text
            {
                Data = text, Start = start, FontFamily = Font.FontFamily.Name, Size = Font.Size * 7 / 5
                , Fill = Stroke
            };
        }

        Content CreateLine(Point start, Point end) => new Path
        {
            PathData = start.Format(new Size(end.X - start.X, end.Y - start.Y).LineTo()), Stroke = Stroke
            , StrokeWidth = Tick
        };

        Size TextSize(string nodeName)
        {
            var result = Graphics.MeasureString(nodeName, Font);
            return new Size((int)result.Width, (int)result.Height);
        }

        Size NodeSize(string nodeName)
        {
            var textSize = TextSize(nodeName);
            return new Size(Math.Max(textSize.Width, SizeBase) + SizeBase, SizeBase * 2);
        }
    }
}