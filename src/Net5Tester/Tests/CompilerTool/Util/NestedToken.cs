using Net5Tester.CompilerTool.Util;

namespace Net5Tester.Tests.CompilerTool.Util
{
    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name) { }

        public override bool IsMain => false;
    }
}