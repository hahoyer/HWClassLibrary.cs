using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class NamedTreeSyntax : TreeSyntax
    {
        readonly NamedToken _tokenClass;

        public NamedTreeSyntax(Syntax left, NamedToken tokenClass, IToken part, Syntax right)
            : base(left, part, right) { _tokenClass = tokenClass; }

        public override string TokenClassName => _tokenClass.Name;
        public override bool TokenClassIsMain => _tokenClass.IsMain;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
            => new RightParenthesisSyntax(id, this, token, right);
    }
}