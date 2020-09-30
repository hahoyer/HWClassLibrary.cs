using System.Drawing;
using System.Xml.Serialization;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Graphics.SVG
{
    public sealed class Path : Content
    {
        internal sealed class Arc : Element
        {
            [EnableDump]
            readonly Size End;

            [EnableDump]
            readonly bool LargeArcFlag;

            [EnableDump]
            readonly Size Radii;

            [EnableDump]
            readonly bool SweepFlag;

            [EnableDump]
            readonly int XAxisRotation;

            public Arc(Size radii, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
                : base(true)
            {
                Radii = radii;
                End = end;
                XAxisRotation = xAxisRotation;
                LargeArcFlag = largeArcFlag;
                SweepFlag = sweepFlag;
            }

            internal override Size Size => End;

            internal override string FormatElement => "a" + Radii.FormatPair() + " " + XAxisRotation + " " +
                                                      (LargeArcFlag? 1 : 0) + " " + (SweepFlag? 1 : 0) + " " +
                                                      FormatSize;
        }

        internal sealed class Line : Element
        {
            [EnableDump]
            readonly Size End;

            public Line(Size end, bool isVisible = true)
                : base(isVisible)
                => End = end;

            internal override Size Size => End;

            internal override string FormatElement => (IsVisible? "l" : "m") + FormatSize;
        }

        internal sealed class HorizontalLine : Element
        {
            [EnableDump]
            readonly int Length;

            public HorizontalLine(int length, bool isVisible = true)
                : base(isVisible)
                => Length = length;

            internal override Size Size => new Size(Length, 0);
            internal override string FormatElement => IsVisible? "h" + Length : "m" + FormatSize;
        }

        internal sealed class VerticalLine : Element
        {
            [EnableDump]
            readonly int Length;

            public VerticalLine(int length, bool isVisible = true)
                : base(isVisible)
                => Length = length;

            internal override Size Size => new Size(0, Length);
            internal override string FormatElement => IsVisible? "v" + Length : "m" + FormatSize;
        }

        internal abstract class Element : DumpableObject
        {
            protected readonly bool IsVisible;

            protected Element(bool isVisible) => IsVisible = isVisible;

            internal abstract Size Size { get; }
            internal abstract string FormatElement { get; }
            internal string FormatSize => Size.FormatPair();
        }

        [PublicAPI]
        sealed class Home : Element
        {
            public Home()
                : base(false) { }

            internal override Size Size => new Size(0, 0);

            internal override string FormatElement => "M" + FormatSize;
        }

        [XmlAttribute("d")]
        public string PathData;
    }

    static class PathExtension
    {
        internal static Path.Element MoveTo(this Size end) => new Path.Line(end, false);
        internal static Path.Element MoveTo(this Point end) => new Path.Line(new Size(end.X, end.Y), false);
        internal static Path.Element LineTo(this Size end) => new Path.Line(end);
        internal static Path.Element HorizontalLine(this int length) => new Path.HorizontalLine(length);
        internal static Path.Element VerticalLine(this int length) => new Path.VerticalLine(length);

        internal static Path.Element Arc
            (this Size radii, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
            => new Path.Arc(radii, end, largeArcFlag, sweepFlag, xAxisRotation);

        internal static Path.Element Arc
            (this int radius, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
            => new Path.Arc(new Size(radius, radius), end, largeArcFlag, sweepFlag, xAxisRotation);

        internal static string Format(this Point start, params Path.Element[] path)
        {
            var current = start;
            var result = "M" + current.FormatPair();
            foreach(var t in path)
            {
                result += " ";
                result += t.FormatElement;
                current += t.Size;
            }

            return result;
        }

        internal static string CloseAndFormat
            (this Point start, params Path.Element[] path) => Format(start, path) + " Z";

        internal static string FormatPair(this Size size) => size.Width + "," + size.Height;
        internal static string FormatPair(this Point point) => point.X + "," + point.Y;
    }
}