using System.Xml.Serialization;

namespace hw.Graphics.SVG
{
    public abstract class Content
    {
        [XmlAttribute("fill")]
        public string Fill;

        [XmlAttribute("stroke")]
        public string Stroke;

        [XmlAttribute("stroke-width")]
        public float StrokeWidth;
    }
}