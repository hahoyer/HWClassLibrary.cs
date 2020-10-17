using System.Xml.Serialization;
// ReSharper disable CheckNamespace

namespace hw.Graphics.SVG
{
    public sealed class Circle : Content
    {
        [XmlAttribute("cx")]
        public int Cx;

        [XmlAttribute("cy")]
        public int Cy;

        [XmlAttribute("r")]
        public int R;
    }
}