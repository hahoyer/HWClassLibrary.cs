using System.Xml.Serialization;
// ReSharper disable CheckNamespace

namespace hw.Graphics.SVG
{
    public sealed class Rect : Content
    {
        [XmlAttribute("height")]
        public int Height;

        [XmlAttribute("width")]
        public int Width;

        [XmlAttribute("target")]
        public int X;

        [XmlAttribute("y")]
        public int Y;
    }
}