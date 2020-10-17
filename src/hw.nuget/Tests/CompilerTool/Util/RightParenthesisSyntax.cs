using System.Diagnostics;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    [DebuggerDisplay("{" + nameof(NodeDump) + "}")]
    sealed class RightParenthesisSyntax : TreeSyntax
    {
        // ReSharper disable once NotAccessedField.Local
        readonly string Id;

        public RightParenthesisSyntax(string id, Syntax left, IToken part, Syntax right)
            : base(left, part, right)
            => Id = id;

        public override string TokenClassName => "?)?";

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}