using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class IntegerSyntax : ParsedSyntax, IComparableEx<IntegerSyntax>
    {
        public IntegerSyntax(IToken token)
            : base(token) { }

        [DisableDump]
        internal override Set<string> Variables => new();

        internal override string SmartDump(ISmartDumpToken @operator) => Token.Characters.Id;
        public int CompareToEx(IntegerSyntax other) => 0;
    }
}