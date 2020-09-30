using System.Xml.Serialization;

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