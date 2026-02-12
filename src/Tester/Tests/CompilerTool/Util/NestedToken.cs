// ReSharper disable CheckNamespace
namespace Tester.Tests.CompilerTool.Util
{
    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name) { }

        public override bool IsMain => false;
    }
}