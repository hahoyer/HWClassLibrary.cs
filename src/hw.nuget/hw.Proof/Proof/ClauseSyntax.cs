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
using hw.Parser;

namespace hw.Proof
{
    sealed class ClauseSyntax : AssociativeSyntax
    {
        public ClauseSyntax(IAssociative @operator, SourcePart token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { }

        public IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(int variableCount) { return Set.Where(parsedSyntax => parsedSyntax.Variables.Count() <= variableCount).SelectMany(GetDefinitions).Where(pair => pair.Value != null); }

        static IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(ParsedSyntax parsedSyntax) { return parsedSyntax.Variables.Select(parsedSyntax.GetDefinition); }

        public string SmartDump() { return SmartDump(null); }
        public ClauseSyntax IsolateAndReplace(int variableCount) { return Apply(new IsolateAndReplaceStrategy(this, variableCount)); }

        ClauseSyntax Apply(IStrategy strategy) { return (ClauseSyntax) Operator.CombineAssosiative(Token, Set | Set.SelectMany(strategy.Apply).ToSet()); }
    }
}