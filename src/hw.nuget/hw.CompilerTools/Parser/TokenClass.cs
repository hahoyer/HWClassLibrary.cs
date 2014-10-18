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

        IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart part, IParsedSyntax right, bool isMatch)
        {
            if(AcceptsMatch == isMatch)
                return Create(left, part, right);
            return Mismatch(left, part, right);
        }

        string IType<IParsedSyntax>.PrioTableName { get { return Name; } }
        bool IType<IParsedSyntax>.IsEnd { get { return IsEnd; } }

        protected virtual IParsedSyntax Mismatch(IParsedSyntax left, IPart part, IParsedSyntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }

        protected virtual bool AcceptsMatch { get { return false; } }
        protected abstract IParsedSyntax Create(IParsedSyntax left, IPart part, IParsedSyntax right);

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Name.Quote() + ")"; }

        [Node]
        [DisableDump]
        internal string Name { get { return _name; } set { _name = value; } }

        [DisableDump]
        protected virtual bool IsEnd { get { return false; } }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }
    }
}