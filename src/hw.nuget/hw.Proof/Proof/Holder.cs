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
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class Holder : Dumpable
    {
        static readonly ParserInst _parser = new ParserInst(new Scanner(), Main.TokenFactory);
        readonly string _text;
        readonly ClauseSyntax _statement;

        public Holder(string text)
        {
            var file = "main.proof".FileHandle();
            file.String = text;
            _text = text;
            var parsedSyntax = _parser.Compile(new Source(file));
            _statement = (ClauseSyntax) parsedSyntax;
        }

        internal Set<string> Variables { get { return _statement.Variables; } }
        internal ClauseSyntax Statement { get { return _statement; } }
    }
}