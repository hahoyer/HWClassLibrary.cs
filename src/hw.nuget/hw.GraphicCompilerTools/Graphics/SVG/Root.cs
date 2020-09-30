using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace hw.Graphics.SVG
{
    [XmlRoot("div")]
    public sealed class Root
    {
        public SVG Svg;

        public string SerializeObject()
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = true};
            var writer = XmlWriter.Create(sb, settings);
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            new XmlSerializer(typeof(Root)).Serialize(writer, this, namespaces);
            writer.Close();
            var result = sb.ToString();
            return result;
        }

        public static Root CreateFromXML(string xml)
        {
            var fs = new StringReader(xml);
            var target = new XmlSerializer(typeof(Root));
            return (Root)target.Deserialize(fs);
        }

        public static Root Create(IGraphTarget target) => new SyntaxDrawer(target).Draw();
    }
}