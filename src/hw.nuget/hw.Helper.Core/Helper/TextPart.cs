using JetBrains.Annotations;

namespace hw.Helper
{
    [PublicAPI]
    public class TextPart
    {
        public TextPosition Start;
        public TextPosition End;
    }

    [PublicAPI]
    public sealed class TextPosition
    {
        public int LineNumber;
        public int ColumnNumber;
    }
}