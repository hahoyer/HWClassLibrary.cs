using System.Xml.Serialization;

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