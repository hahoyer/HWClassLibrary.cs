using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using hw.DebugFormatter;
// ReSharper disable CheckNamespace

namespace hw.Graphics
{
    sealed class SyntaxDrawer
        : DumpableObject
            , ISyntaxDrawer
    {
        readonly Bitmap Bitmap;
        readonly Font Font;
        readonly System.Drawing.Graphics Graphics;
        readonly SolidBrush LineBrush;
        readonly Pen LinePen;
        readonly SolidBrush NodeBrush;
        readonly int SizeBase;
        readonly StringFormat StringFormat;
        readonly Syntax Syntax;

        SyntaxDrawer(IGraphTarget target)
        {
            StringFormat = new(StringFormatFlags.NoWrap);
            Font = new(FontFamily.Families.Single(f1 => f1.Name == "Arial"), 10);
            LineBrush = new(Color.Black);
            LinePen = new(LineBrush, 1);
            NodeBrush = new(Color.LightBlue);
            Graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            SizeBase = Font.Height * 8 / 10;

            Syntax = Syntax.Create(target, this);
            var width = Syntax.Width + SizeBase + 1;
            var height = Syntax.Height + SizeBase + 1;
            Bitmap = new(width, height);
            Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            var frame = new Rectangle(0, 0, width, height);
            Graphics.FillRectangle(new SolidBrush(Color.Transparent), frame);
            Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        void ISyntaxDrawer.DrawLine(Point start, Point end) => Graphics.DrawLine(LinePen, start, end);

        void ISyntaxDrawer.DrawNode(Point origin, string nodeName)
        {
            var size = NodeSize(nodeName);
            var arcSize = new Size(2 * SizeBase, 2 * SizeBase);
            var bodyWidth = new Size(size.Width - 2 * SizeBase, 0);
            var lineOrigin = origin + new Size(SizeBase, 0);

            var r = new GraphicsPath();
            r.AddArc(new(origin, arcSize), 90, 180);
            r.AddLine(lineOrigin, lineOrigin + bodyWidth);
            r.AddArc(new(origin + bodyWidth, arcSize), 270, 180);
            r.AddLine(lineOrigin + bodyWidth + new Size(0, SizeBase * 2), lineOrigin + new Size(0, SizeBase * 2));
            Graphics.FillPath(NodeBrush, r);
            Graphics.DrawPath(LinePen, r);

            var s = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center};
            Graphics.DrawString(nodeName, Font, LineBrush, new Rectangle(origin, size), s);
        }

        Size ISyntaxDrawer.Gap => new(SizeBase, SizeBase);
        int ISyntaxDrawer.NodeHeight(string nodeName) => SizeBase * 2;
        int ISyntaxDrawer.NodeWidth(string nodeName) => Math.Max(TextWidth(nodeName), SizeBase) + SizeBase;

        internal static Image DrawBitmap(IGraphTarget syntax) => new SyntaxDrawer(syntax).Draw();

        Image Draw()
        {
            Syntax.Draw(new(SizeBase / 2, SizeBase / 2));
            return Bitmap;
        }

        int TextWidth
            (string nodeName) => (int)Graphics.MeasureString(nodeName, Font, new PointF(0, 0), StringFormat).Width;

        Size NodeSize
            (string nodeName) => new(((ISyntaxDrawer)this).NodeWidth(nodeName)
            , ((ISyntaxDrawer)this).NodeHeight(nodeName));
    }
}