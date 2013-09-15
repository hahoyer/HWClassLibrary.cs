#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    sealed class FactorSyntax : ParsedSyntax, IComparableEx<FactorSyntax>
    {
        internal readonly ParsedSyntax Target;
        internal readonly BigRational Value;

        public FactorSyntax(ParsedSyntax target, BigRational value)
            : base(null)
        {
            Target = target;
            Value = value;
            Tracer.Assert(Target.SmartDumpText != "0");
        }

        int IComparableEx<FactorSyntax>.CompareToEx(FactorSyntax other)
        {
            var result = Value.CompareTo(other.Value);
            if(result == 0)
                result = Target.CompareTo(other.Target);
            return result;
        }

        [DisableDump]
        internal override Set<string> Variables { get { return Target.Variables; } }

        [DisableDump]
        internal override bool IsNegative { get { return Target.IsNegative != Value.IsNegative; } }

        [DisableDump]
        internal override BigRational Factor { get { return Value * Target.Factor; } }

        internal override string SmartDump(ISmartDumpToken @operator) { return SmartDumpFactor(@operator) + Target.SmartDump(@operator); }
        internal override ParsedSyntax Times(BigRational value) { return Target.Times(value * Value); }

        internal override ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other) { return Target.IsolateFromSum(variable, other.Times(1 / Value)); }
        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions) { return Target.Replace(definitions).Select(x => x.Times(Value)).ToSet(); }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other) { return other.CombineForPlus(Target, Value); }
        internal override ParsedSyntax CombineForPlus(VariableSyntax other) { return Target.CombineForPlus(other, Value); }
        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue) { return other.CombineForPlus(Target, Value, otherValue); }
        internal override ParsedSyntax CombineForPlus(PowerSyntax other) { return Target.CombineForPlus(other, Value); }

        string SmartDumpFactor(ISmartDumpToken @operator)
        {
            Tracer.Assert(Value.Nominator != 0);
            Tracer.Assert(Value.Nominator != Value.Denominator);
            Tracer.Assert(Value.Denominator > 0);
            if(Value.IsNegative && @operator != null && @operator.IsIgnoreSignSituation)
                return Value == -1 ? "" : (-Value).ToString();

            var result = Value.ToString();
            if(Value.IsNegative)
                result = "(" + result + ")";
            return result + "*";
        }
    }
}