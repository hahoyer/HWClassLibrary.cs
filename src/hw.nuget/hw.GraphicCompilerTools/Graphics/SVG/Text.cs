using System.Drawing;
using System.Xml.Serialization;

namespace hw.Graphics.SVG
{
    public sealed class Text : Content
    {
        [XmlText]
        public string Data;

        [XmlAttribute("font-family")]
        public string FontFamily;

        [XmlAttribute("font-size")]
        public float Size;

        internal Point Start;

        [XmlAttribute("X")]
        public int StartX
        {
            get => Start.X;
            set => Start.X = value;
        }

        [XmlAttribute("Y")]
        public int StartY
        {
            get => Start.Y;
            set => Start.Y = value;
        }
    }
}