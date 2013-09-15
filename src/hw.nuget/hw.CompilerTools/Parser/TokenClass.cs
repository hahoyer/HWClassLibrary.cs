#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.PrioParser;
using hw.TreeStructure;

namespace hw.Parser
{
    abstract class TokenClass : DumpableObject, IIconKeyProvider, ITokenClass
    {
        static int _nextObjectId;
        string _name;

        protected TokenClass()
            : base(_nextObjectId++) { StopByObjectId(-31); }

        string IIconKeyProvider.IconKey { get { return "Symbol"; } }

        [EnableDumpExcept(null)]
        protected virtual ITokenFactory NewTokenFactory { get { return null; } }

        string INameProvider.Name { set { Name = value; } }

        IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart<IParsedSyntax> part, IParsedSyntax right) { return Create(left, part, right); }
        string IType<IParsedSyntax>.PrioTableName { get { return Name; } }
        bool IType<IParsedSyntax>.IsEnd { get { return IsEnd; } }

        protected abstract IParsedSyntax Create(IParsedSyntax left, IPart<IParsedSyntax> part, IParsedSyntax right);

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Name.Quote() + ")"; }

        [Node]
        [DisableDump]
        internal string Name { get { return _name; } set { _name = value; } }

        [DisableDump]
        protected virtual bool IsEnd { get { return false; } }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }
    }
}