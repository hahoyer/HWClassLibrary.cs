using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.Parser
{
    /// <summary>
    ///     Priority table used in parsing to create the syntax tree.
    /// </summary>
    [PublicAPI]
    public sealed class PrioTable : DumpableObject
    {
        public interface ITargetItem
        {
            string Token { get; }
            BracketContext LeftContext { get; }
        }

        public sealed class RelationDefinitionItem
        {
            public readonly string[] Token;
            internal readonly bool IsRight;

            public RelationDefinitionItem(bool isRight, string[] token)
            {
                IsRight = isRight;
                Token = token;
            }
        }

        public sealed class BracketPairItem
        {
            public readonly string Left;
            public readonly string Right;

            public BracketPairItem(string left, string right)
            {
                Left = left;
                Right = right;
            }
        }

        public sealed class Relation : EnumEx
        {
            public static readonly Relation Push = new Relation
                {IsPush = true};

            public static readonly Relation Pull = new Relation
                {IsPull = true};

            public static readonly Relation Match = new Relation
            {
                IsPull = true, IsBracket = true, IsMatch = true
            };

            public static readonly Relation Mismatch = new Relation
            {
                IsPull = true, IsBracket = true
            };

            public bool IsBracket;
            public bool IsMatch;

            public bool IsPull;
            public bool IsPush;
        }

        enum FunctionType
        {
            Unknown
            , Push
            , Relation
            , Pull
            , Match
        }

        public const string Any = "(any)";
        public const string EndOfText = "(eot)";
        public const string BeginOfText = "(bot)";
        public const string Error = "(err)";
        public const string LeftBracket = "(left)";
        public const string RightBracket = "(right)";
        public const string Bracket = "(bracket)";

        static readonly FunctionType[][] SameDepth =
        {
            new[] {FunctionType.Push, FunctionType.Push, FunctionType.Push}
            , new[] {FunctionType.Push, FunctionType.Relation, FunctionType.Relation}
            , new[] {FunctionType.Match, FunctionType.Pull, FunctionType.Pull}
        };

        public string Title;

        readonly RelationDefinitionItem[] BaseRelation;
        BracketContext BracketContextValue;
        readonly BracketPairItem[] Brackets;

        PrioTable()
        {
            BaseRelation = new RelationDefinitionItem[] { };
            Brackets = new BracketPairItem[] { };
        }

        PrioTable(bool isRight, string[] token)
            : this() => BaseRelation = new[] {new RelationDefinitionItem(isRight, token)};

        PrioTable(PrioTable target, PrioTable y)
            : this()
        {
            BaseRelation = target.BaseRelation.Concat(y.BaseRelation).ToArray();
            Brackets = target.Brackets.Concat(y.Brackets).ToArray();
        }

        PrioTable(BracketPairItem bracketPairItem)
            : this() => Brackets = new[] {bracketPairItem};

        [DisableDump]
        public BracketContext BracketContext
            => BracketContextValue ?? (BracketContextValue = BracketContext.Instance(Brackets));

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are left associative
        /// </summary>
        /// <param name="target"> </param>
        /// <returns> </returns>
        public static PrioTable Left(params string[] target) => new PrioTable(false, target);

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are right associative
        /// </summary>
        /// <param name="target"> </param>
        /// <returns> </returns>
        public static PrioTable Right(params string[] target) => new PrioTable(true, target);

        public static PrioTable BracketParallels(string[] leftBrackets, string[] rightBrackets)
        {
            Tracer.Assert(leftBrackets.Length == rightBrackets.Length);
            return leftBrackets
                .Select((item, index) => BracketPair(item, rightBrackets[index]))
                .Aggregate(new PrioTable(), (c, n) => new PrioTable(c, n));
        }

        public static PrioTable BracketPair(string left, string right)
            => new PrioTable(new BracketPairItem(left, right));

        public static PrioTable FromText(string text) => FromText(text.Split('\n', '\r'));

        public static PrioTable operator +(PrioTable target, PrioTable y) => new PrioTable(target, y);

        public static bool operator ==(PrioTable target, PrioTable y)
            => target?.Equals(y) ?? ReferenceEquals(y, null);

        public static bool operator !=(PrioTable target, PrioTable y)
            => !target?.Equals(y) ?? !ReferenceEquals(y, null);

        public override bool Equals(object obj)
        {
            var target = obj as PrioTable;
            if(target == null)
                return false;
            return BaseRelation == target.BaseRelation && Brackets == target.Brackets;
        }

        public override int GetHashCode() => BaseRelation.GetHashCode() + Brackets.GetHashCode();

        public override string ToString() => Title ?? Dump();

        public Relation GetRelation(ITargetItem newItem, ITargetItem recentItem)
        {
            var delta = newItem.GetLeftDepth() - recentItem.GetRightDepth();

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

        static PrioTable FromText(string[] text)
            => FromText(text.Select(l => l.Split(' ', '\t')).ToArray());

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
                    // ReSharper disable once StringLiteralTypo
                    case "parlevel":
                        result += BracketParallels
                        (
                            data.Take(tokenCount).ToArray(),
                            data.Skip(tokenCount).Take(tokenCount).ToArray());
                        break;
                    case "":
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            result += BracketParallels(new[] {BeginOfText}, new[] {EndOfText});
            return result;
        }

        int? BaseRelationIndex(string token) => BaseRelation.IndexWhere(item => item.Token.Contains(token));

        int GetSubTypeIndex(ITargetItem target)
        {
            var parent = this;
            Tracer.Assert(!string.IsNullOrWhiteSpace(target.Token));
            var token = target.Token;
            return parent.Brackets.Any(item => item.Left == token)? 0 :
                parent.Brackets.Any(item => item.Right == token)? 2 : 1;
        }

        Relation NotImplemented(string newToken, string recentToken)
        {
            NotImplementedMethod(newToken, recentToken);
            return null;
        }

        Relation GetRelationOnSameDepth(int newIndex, int otherIndex, string newToken, string otherToken)
        {
            switch(SameDepth[newIndex][otherIndex])
            {
                case FunctionType.Unknown:
                    return NotImplemented(newToken, otherToken);
                case FunctionType.Push:
                    return Relation.Push;
                case FunctionType.Relation:
                    return GetRelation(newIndex, otherIndex, newToken, otherToken);
                case FunctionType.Pull:
                    return Relation.Pull;
                case FunctionType.Match:
                    return GetBracketMatch(newToken, otherToken);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Relation GetBracketMatch(string newToken, string otherToken)
        {
            var newIndex = GetRightBracketIndex(newToken);
            var otherIndex = GetLeftBracketIndex(otherToken);
            return newIndex == otherIndex? Relation.Match :
                newIndex > otherIndex? Relation.Mismatch : Relation.Push;
        }

        int GetLeftBracketIndex(string token)
            => Brackets
                .IndexWhere(item => item.Left == token)
                .AssertValue();

        int GetRightBracketIndex(string token)
            => Brackets
                .IndexWhere(item => item.Right == token)
                .AssertValue();

        Relation GetRelation(int newTypeIndex, int otherTypeIndex, string newToken, string otherToken)
        {
            var index = GetRelationIndex(newToken, newTypeIndex);
            var delta = index - GetRelationIndex(otherToken, otherTypeIndex);
            var isPush = delta == 0? BaseRelation[index].IsRight : delta < 0;
            return isPush
                ? Relation.Push
                : Relation.Pull;
        }

        int GetRelationIndex(string token, int type)
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