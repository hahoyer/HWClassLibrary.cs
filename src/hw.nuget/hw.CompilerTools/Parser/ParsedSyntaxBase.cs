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
using hw.Graphics;
using hw.TreeStructure;
using JetBrains.Annotations;

namespace hw.Parser
{
    abstract class ParsedSyntaxBase : DumpableObject, IParsedSyntax, IGraphTarget
    {
        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        readonly TokenData _token;

        protected ParsedSyntaxBase(TokenData token) { _token = token; }

        protected ParsedSyntaxBase(TokenData token, int nextObjectId)
            : base(nextObjectId) { _token = token; }


        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        string IParsedSyntax.GetNodeDump() { return GetNodeDump(); }

        [DisableDump]
        TokenData IParsedSyntax.Token { get { return Token; } }

        [DisableDump]
        TokenData IParsedSyntax.FirstToken { get { return FirstToken; } }

        [DisableDump]
        TokenData IParsedSyntax.LastToken { get { return LastToken; } }

        [DisableDump]
        internal TokenData Token { get { return _token; } }

        [DisableDump]
        internal virtual TokenData FirstToken { get { return Token; } }

        [DisableDump]
        internal virtual TokenData LastToken { get { return Token; } }

        protected override string GetNodeDump() { return Token.Name; }
        protected virtual string FilePosition() { return Token.FilePosition; }
        internal string FileErrorPosition(string errorTag) { return Token.FileErrorPosition(errorTag); }

        string IGraphTarget.Title { get { return Token.Name; } }
        IGraphTarget[] IGraphTarget.Children { get { return Children.ToArray<IGraphTarget>(); } }

        [DisableDump]
        protected virtual ParsedSyntaxBase[] Children { get { return new ParsedSyntaxBase[0]; } }
    }
}