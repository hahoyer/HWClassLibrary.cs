using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.PrioParser;

namespace hw.Parser
{
    abstract class TokenClass : DumpableObject, IIconKeyProvider, ITokenClass
    {
        static int _nextObjectId;
        string _name;

        protected TokenClass()
            : base(_nextObjectId++) { StopByObjectId(-31); }

        string IIconKeyProvider.IconKey { get { return "Symbol"; } }

        string INameProvider.Name { set { Name = value; } }

        ParsedSyntax IType<ParsedSyntax>.Create(ParsedSyntax left, SourcePart part, ParsedSyntax right, bool isMatch)
        {
            if(AcceptsMatch == isMatch)
                return Create(left, part, right);
            return Mismatch(left, part, right);
        }

        string IType<ParsedSyntax>.PrioTableName { get { return Name; } }
        Control IType<ParsedSyntax>.Next { get { return Next; } }

        protected virtual Control Next { get { return null; } }

        protected virtual ParsedSyntax Mismatch(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }

        protected virtual bool AcceptsMatch { get { return false; } }
        protected abstract ParsedSyntax Create(ParsedSyntax left, SourcePart part, ParsedSyntax right);

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Name.Quote() + ")"; }

        [Node]
        [DisableDump]
        internal string Name { get { return _name; } set { _name = value; } }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }
    }
}