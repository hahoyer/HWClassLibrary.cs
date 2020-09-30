using hw.DebugFormatter;

namespace hw.Tests.CompilerTool.Util
{
    sealed class MainToken : NamedToken
    {
        public MainToken(string name)
            : base(name) { }

        [DisableDump]
        public override bool IsMain => true;
    }
}