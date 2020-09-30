using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class NamedToken : ParserTokenType<Syntax>
    {
        internal readonly string Name;
        protected NamedToken(string name) { Name = name; }
        public abstract bool IsMain { get; }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new NamedTreeSyntax(left, this, token, right);

        public override string Id => Name;
    }
}