using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Helper
{
    public sealed class BigRational : IEquatable<BigRational>
    {
        public readonly BigInteger Nominator;
        public readonly BigInteger Denominator;

        public BigRational(BigInteger nominator, BigInteger denominator)
        {
            var greatestCommonDivisor = GreatestCommonDivisor(nominator, denominator);
            Nominator = nominator/greatestCommonDivisor;
            Denominator = denominator/greatestCommonDivisor;
            Tracer.Assert(Denominator > 0);
        }

        [IsDumpEnabled(false)]
        public bool IsNegative { get { return Nominator < 0; } }

        private static BigInteger GreatestCommonDivisor(BigInteger nominator, BigInteger denominator)
        {
            var sign = 1;
            if(denominator < 0)
                sign = -1;
            return BigInteger.GreatestCommonDivisor(nominator, denominator)*sign;
        }

        public int CompareTo(BigRational other)
        {
            var result = Nominator.CompareTo(other.Nominator);
            if(result == 0)
                result = Denominator.CompareTo(other.Denominator);
            return result;
        }

        public static BigRational operator *(BigRational x, BigRational y) { return new BigRational(x.Nominator*y.Nominator, x.Denominator*y.Denominator); }
        public static BigRational operator /(BigRational x, BigRational y) { return new BigRational(x.Nominator*y.Denominator, x.Denominator*y.Nominator); }
        public static BigRational operator /(int x, BigRational y) { return new BigRational(x * y.Denominator, y.Nominator); }
        public static BigRational operator -(BigRational x) { return new BigRational(-x.Nominator, x.Denominator); }
        public static BigRational operator +(BigRational x, int y) { return new BigRational(x.Nominator + y*x.Denominator, x.Denominator); }
        public static BigRational operator +(BigRational x, BigRational y) { return new BigRational(x.Nominator*y.Denominator + y.Nominator*x.Denominator, x.Denominator*y.Denominator); }

        public override string ToString()
        {
            var result = Nominator.ToString();
            if (Denominator == 1)
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
                return (Nominator.GetHashCode()*397) ^ Denominator.GetHashCode();
            }
        }

        public static bool operator ==(BigRational left, BigRational right) { return Equals(left, right); }
        public static bool operator !=(BigRational left, BigRational right) { return !Equals(left, right); }
        public static bool operator ==(BigRational left, int right) { return left.Nominator == right && left.Denominator == 1; }
        public static bool operator !=(BigRational left, int right) { return !(left == right); }
        private static BigRational Create(int value) { return new BigRational(value,1); }
        private static BigRational Create(BigInteger value) { return new BigRational(value,1);}
        public static implicit operator BigRational(int value) { return Create(value); }
        public static implicit operator BigRational(BigInteger value) { return Create(value); }
    }
}