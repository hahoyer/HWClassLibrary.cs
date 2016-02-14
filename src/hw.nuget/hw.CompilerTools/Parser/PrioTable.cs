using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.Parser
{
    /// <summary>
    ///     Priority table used in parsing to create the syntax tree.
    /// </summary>
    public sealed class PrioTable : DumpableObject
    {
        public string Title;
        public const string Any = "(any)";
        public const string EndOfText = "(eot)";
        public const string BeginOfText = "(bot)";
        public const string Error = "(err)";
        public const string LeftBracket = "(left)";
        public const string RightBracket = "(right)";
        public const string Bracket = "(bracket)";

        public interface ITargetItem
        {
            string Token { get; }
            BracketContext LeftContext { get; }
            int NextDepth { get; }
        }

        internal sealed class RelationDefinitionItem
        {
            public readonly string[] Token;
            internal readonly bool IsRight;

            public RelationDefinitionItem(bool isRight, string[] token)
            {
                IsRight = isRight;
                Token = token;
            }
        }

        internal sealed class BracketPairItem
        {
            public readonly string Left;
            public readonly string Right;

            public BracketPairItem(string left, string right)
            {
                Left = left;
                Right = right;
            }
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are left associative
        /// </summary>
        /// <param name="x"> </param>
        /// <returns> </returns>
        public static PrioTable Left(params string[] x)
        {
            return new PrioTable(false, x);
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are right associative
        /// </summary>
        /// <param name="x"> </param>
        /// <returns> </returns>
        public static PrioTable Right(params string[] x)
        {
            return new PrioTable(true, x);
        }

        public static PrioTable BracketParallels(string[] leftBrackets, string[] rightBrackets)
        {
            Tracer.Assert(leftBrackets.Length == rightBrackets.Length);
            return leftBrackets
                .Select((item, index) => BracketPair(item, rightBrackets[index]))
                .Aggregate(new PrioTable(), (c, n) => new PrioTable(c, n));
        }

        public static PrioTable BracketPair(string left, string right)
        {
            return new PrioTable(new BracketPairItem(left, right));
        }

        public static PrioTable FromText(string text) { return FromText(text.Split('\n', '\r')); }

        [DisableDump]
        internal readonly RelationDefinitionItem[] BaseRelation;
        [DisableDump]
        internal readonly BracketPairItem[] Brackets;

        PrioTable()
        {
            BaseRelation = new RelationDefinitionItem[] {};
            Brackets = new BracketPairItem[] {};
        }

        PrioTable(bool isRight, string[] token)
        {
            BaseRelation = new[] {new RelationDefinitionItem(isRight, token)};
            Brackets = new BracketPairItem[] {};
        }

        PrioTable(PrioTable x, PrioTable y)
        {
            BaseRelation = x.BaseRelation.Concat(y.BaseRelation).ToArray();
            Brackets = x.Brackets.Concat(y.Brackets).ToArray();
        }

        PrioTable(BracketPairItem bracketPairItem)
        {
            BaseRelation = new RelationDefinitionItem[] {};
            Brackets = new[] {bracketPairItem};
        }

        public static PrioTable operator +(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        public static bool operator ==(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return ReferenceEquals(y, null);
            return x.Equals(y);
        }


        internal sealed class Relation : EnumEx
        {
            internal static readonly Relation Push = new Relation
            {
                IsPush = true
            };

            internal static readonly Relation Pull = new Relation
            {
                IsPull = true
            };

            internal static readonly Relation Match = new Relation
            {
                IsPull = true,
                IsBracket = true,
                IsMatch = true
            };

            internal static readonly Relation Mismatch = new Relation
            {
                IsPull = true,
                IsBracket = true
            };

            public bool IsPull;
            public bool IsPush;
            public bool IsBracket;
            public bool IsMatch;
        }

        internal Relation GetRelation(ITargetItem newItem, ITargetItem recentItem)
        {
            var delta = newItem.LeftContext.Depth - recentItem.NextDepth;

            switch(delta)
            {
            case 0:
                return GetRelationOnSameDepth
                    (
                        GetSubTypeIndex(newItem),
                        GetSubTypeIndex(recentItem),
                        newItem.Token,
                        recentItem.Token);
            case 1:
                return Relation.Push;
            default:
                NotImplementedMethod(newItem, recentItem, nameof(delta), delta);
                return null;
            }
        }

        internal BracketContext NextContext(BracketContext context, string token)
            => context.Add(GetContextIndex(token));

        int GetContextIndex(string token)
        {
            if(token == "")
                token = BeginOfText;
            for(var index = 0; index < Brackets.Length; index++)
            {
                var item = Brackets[index];
                if(token == item.Left)
                    return -index - 1;
                if(token == item.Right)
                    return index + 1;
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            var x = obj as PrioTable;
            if(x == null)
                return false;
            return BaseRelation == x.BaseRelation && Brackets == x.Brackets;
        }

        public override int GetHashCode()
        {
            return BaseRelation.GetHashCode() + Brackets.GetHashCode();
        }

        public static bool operator !=(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return !ReferenceEquals(y, null);
            return !x.Equals(y);
        }

        public override string ToString() { return Title ?? Dump(); }

        static PrioTable FromText(string[] text)
        {
            return FromText(text.Select(l => l.Split(' ', '\t')).ToArray());
        }

        static PrioTable FromText(string[][] text)
        {
            var result = Left(Any);
            foreach(var line in text)
            {
                var data = line.Skip(1).ToArray();
                var tokenCount = data.Length / 2;
                switch(line[0].ToLowerInvariant())
                {
                case "left":
                    result += Left(data);
                    break;
                case "right":
                    result += Right(data);
                    break;
                case "parlevel":
                    result += BracketParallels
                        (
                            data.Take(tokenCount).ToArray(),
                            data.Skip(tokenCount).Take(tokenCount).ToArray());
                    break;
                default:
                    throw new ArgumentException();
                }
            }

            result += BracketParallels(new[] {BeginOfText}, new[] {EndOfText});
            return result;
        }

        internal int? BaseRelationIndex(string token)
        {
            return BaseRelation.IndexWhere(item => item.Token.Contains(token));
        }

        internal int GetSubTypeIndex(ITargetItem target)
        {
            var parent = this;
            Tracer.Assert(!string.IsNullOrWhiteSpace(target.Token));
            var token = target.Token;
            if(parent.Brackets.Any(item => item.Left == token))
                return 0;
            if(parent.Brackets.Any(item => item.Right == token))
                return 2;
            return 1;
        }

        enum FunctionType
        {
            Unkown,
            Push,
            Relation,
            Pull,
            Match
        }

        static readonly FunctionType[][] SameDepth =
        {
            new[] {FunctionType.Push, FunctionType.Push, FunctionType.Push},
            new[] {FunctionType.Push, FunctionType.Relation, FunctionType.Relation},
            new[] {FunctionType.Match, FunctionType.Pull, FunctionType.Pull}
        };

        Relation NotImplemented(string newToken, string recentToken)
        {
            NotImplementedMethod(newToken, recentToken);
            return null;
        }

        internal Relation GetRelationOnSameDepth
            (int newIndex, int otherIndex, string newToken, string otherToken)
        {
            switch(SameDepth[newIndex][otherIndex])
            {
            case FunctionType.Unkown:
                return NotImplemented(newToken, otherToken);
            case FunctionType.Push:
                return Relation.Push;
            case FunctionType.Relation:
                return GetRelation(newIndex, otherIndex, newToken, otherToken);
            case FunctionType.Pull:
                return Relation.Pull;
            case FunctionType.Match:
                return GetBracketMatch(newToken, otherToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        Relation GetBracketMatch(string newToken, string otherToken)
        {
            var newIndex = GetRightBracketIndex(newToken);
            var otherIndex = GetLeftBracketIndex(otherToken);
            if(newIndex == otherIndex)
                return Relation.Match;
            if(newIndex > otherIndex)
                return Relation.Mismatch;
            return Relation.Push;
        }

        internal int GetLeftBracketIndex(string token)
            => Brackets
                .IndexWhere(item => item.Left == token)
                .AssertValue();

        internal int GetRightBracketIndex(string token)
            => Brackets
                .IndexWhere(item => item.Right == token)
                .AssertValue();

        Relation GetRelation
            (int newTypeIndex, int otherTypeIndex, string newToken, string otherToken)
        {
            var index = GetRelationIndex(newToken, newTypeIndex);
            var delta = index - GetRelationIndex(otherToken, otherTypeIndex);
            var isPush = delta == 0 ? BaseRelation[index].IsRight : delta < 0;
            return isPush
                ? Relation.Push
                : Relation.Pull;
        }

        internal int GetRelationIndex(string token, int type)
        {
            var result = BaseRelationIndex(token);
            if(type == 0)
                result = result ?? BaseRelationIndex(LeftBracket);
            if(type == 2)
                result = result ?? BaseRelationIndex(RightBracket);
            if(type != 1)
                result = result ?? BaseRelationIndex(Bracket);
            result = result ?? BaseRelationIndex(Any);
            return result.AssertValue();
        }
    }
}