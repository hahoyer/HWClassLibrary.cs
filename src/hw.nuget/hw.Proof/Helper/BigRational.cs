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
using System.Numerics;
using hw.DebugFormatter;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Helper
{
    [Dump("ToString")]
    public sealed class BigRational : IEquatable<BigRational>
    {
        [DisableDump]
        public readonly BigInteger Nominator;
        [DisableDump]
        public readonly BigInteger Denominator;

        public BigRational(BigInteger nominator, BigInteger denominator)
        {
            var greatestCommonDivisor = GreatestCommonDivisor(nominator, denominator);
            Nominator = nominator / greatestCommonDivisor;
            Denominator = denominator / greatestCommonDivisor;
            (Denominator > 0).Assert();
        }

        [DisableDump]
        public bool IsNegative { get { return Nominator < 0; } }

        static BigInteger GreatestCommonDivisor(BigInteger nominator, BigInteger denominator)
        {
            var sign = 1;
            if(denominator < 0)
                sign = -1;
            return BigInteger.GreatestCommonDivisor(nominator, denominator) * sign;
        }

        public int CompareTo(BigRational other)
        {
            var result = Nominator.CompareTo(other.Nominator);
            if(result == 0)
                result = Denominator.CompareTo(other.Denominator);
            return result;
        }

        public static BigRational operator *(BigRational target, BigRational y) { return new(target.Nominator * y.Nominator, target.Denominator * y.Denominator); }
        public static BigRational operator /(BigRational target, BigRational y) { return new(target.Nominator * y.Denominator, target.Denominator * y.Nominator); }
        public static BigRational operator /(int target, BigRational y) { return new(target * y.Denominator, y.Nominator); }
        public static BigRational operator -(BigRational target) { return new(-target.Nominator, target.Denominator); }
        public static BigRational operator +(BigRational target, int y) { return new(target.Nominator + y * target.Denominator, target.Denominator); }
        public static BigRational operator +(BigRational target, BigRational y) { return new(target.Nominator * y.Denominator + y.Nominator * target.Denominator, target.Denominator * y.Denominator); }

        public override string ToString()
        {
            var result = Nominator.ToString();
            if(Denominator == 1)
                return result;
            return result + "/" + Denominator;
        }

        public bool Equals(BigRational other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;
            return other.Nominator.Equals(Nominator) && other.Denominator.Equals(Denominator);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != typeof(BigRational))
                return false;
            return Equals((BigRational) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Nominator.GetHashCode() * 397) ^ Denominator.GetHashCode();
            }
        }

        public static bool operator ==(BigRational left, BigRational right) { return Equals(left, right); }
        public static bool operator !=(BigRational left, BigRational right) { return !Equals(left, right); }
        public static bool operator ==([NotNull] BigRational left, int right) { return left.Nominator == right && left.Denominator == 1; }
        public static bool operator !=([NotNull] BigRational left, int right) { return !(left == right); }
        static BigRational Create(int value) { return new(value, 1); }
        static BigRational Create(BigInteger value) { return new(value, 1); }
        public static implicit operator BigRational(int value) { return Create(value); }
        public static implicit operator BigRational(BigInteger value) { return Create(value); }
    }
}