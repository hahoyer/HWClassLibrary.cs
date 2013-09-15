#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using HWClassLibrary.Debug;
using HWClassLibrary.Parser;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class SyntaxError : Dumpable, IType<IParsedSyntax>, Match.IError
    {
        [EnableDump]
        readonly IssueId _issueId;

        public SyntaxError(IssueId issueId) { _issueId = issueId; }

        IParsedSyntax IType<IParsedSyntax>.Create(IParsedSyntax left, IPart<IParsedSyntax> part, IParsedSyntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        string IType<IParsedSyntax>.PrioTableName { get { return PrioTable.Error; } }
        bool IType<IParsedSyntax>.IsEnd { get { return false; } }
    }
}