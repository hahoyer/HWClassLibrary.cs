using System.Collections.Generic;
using System.Xml.Serialization;

namespace hw.Graphics.SVG
{
    public sealed class SVG
    {
        [XmlAttribute("height")]
        public string Height;

        [XmlAttribute("viewbox")]
        public string ViewBox;

        [XmlAttribute("width")]
        public string Width;

        [XmlElement("rect", typeof(Rect))]
        [XmlElement("circle", typeof(Circle))]
        [XmlElement("path", typeof(Path))]
        [XmlElement("text", typeof(Text))]
        public List<Content> Items { get; } = new List<Content>();
    }
}