using hw.DebugFormatter;
// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool.Util
{
    sealed class MainToken : NamedToken
    {
        public MainToken(string name)
            : base(name) { }

        [DisableDump]
        public override bool IsMain => true;
    }
}