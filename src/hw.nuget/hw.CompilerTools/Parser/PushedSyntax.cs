﻿#region Copyright (C) 2013

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
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    ///     Incomplete syntax tree element
    /// </summary>
    [Obsolete("", true)]
    sealed class PushedSyntax : Dumpable
    {
        readonly IParsedSyntax _left;
        readonly Item<IParsedSyntax> _token;
        readonly ITokenFactory _tokenFactory;

        internal PushedSyntax(IParsedSyntax left, Item<IParsedSyntax> token, ITokenFactory tokenFactory)
        {
            _left = left;
            _token = token;
            _tokenFactory = tokenFactory;
        }

        internal PushedSyntax(SourcePosn sourcePosn, ITokenFactory tokenFactory)
            : this(null, null, tokenFactory) { }

        internal ITokenFactory TokenFactory { get { return _tokenFactory; } }

        internal char Relation(string newTokenName) { return _tokenFactory.PrioTable.Relation(newTokenName, _token.Name); }

        public IParsedSyntax Syntax(IParsedSyntax args)
        {
            NotImplementedMethod(args);
            return null;
        }

        public override string ToString()
        {
            if(_left == null)
                return "null " + _token.Name;
            return _left.GetNodeDump() + " " + _token.Name + " " + _tokenFactory;
        }
    }
}