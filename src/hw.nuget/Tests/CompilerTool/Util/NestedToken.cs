using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Tests.CompilerTool.Util
{
    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name) { }

        public override bool IsMain => false;
    }
}