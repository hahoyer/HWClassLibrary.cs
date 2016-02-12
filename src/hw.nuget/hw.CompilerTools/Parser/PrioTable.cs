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

        public sealed class Context : DumpableObject
        {
            internal static readonly Context Empty = new Context(new int[] {});

            [EnableDump]
            readonly int[] Data;

            Context(int[] data) { Data = data; }

            internal int Depth => Data.Length;

            public Context Add(int index)
            {
                if(index == 0)
                    return this;

                var xx = Data.SkipWhile(item => item < 0 && item + index > 0).ToArray();
                if(index > 0 && xx.FirstOrDefault() + index == 0)
                    return new Context(xx.Skip(1).ToArray());
                return new Context(new[] {index}.Concat(xx).ToArray());
            }
        }

        public interface IItem
        {
            string Token { get; }
            Context Context { get; }
        }

        sealed class Item
        {
            public readonly string[] Token;
            internal readonly bool IsRight;

            public Item(bool isRight, string[] token)
            {
                IsRight = isRight;
                Token = token;
            }
        }

        sealed class BracketPairItem
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

        readonly Item[] BaseRelation;
        readonly BracketPairItem[] Brackets;
        readonly bool NormalOnRightBracket;

        PrioTable()
        {
            BaseRelation = new Item[] {};
            Brackets = new BracketPairItem[] {};
        }

        PrioTable(bool isRight, string[] token)
        {
            BaseRelation = new[] {new Item(isRight, token)};
            Brackets = new BracketPairItem[] {};
        }

        PrioTable(PrioTable x, PrioTable y)
        {
            NormalOnRightBracket = x.NormalOnRightBracket;
            BaseRelation = x.BaseRelation.Concat(y.BaseRelation).ToArray();
            Brackets = x.Brackets.Concat(y.Brackets).ToArray();
        }

        PrioTable(BracketPairItem bracketPairItem)
        {
            BaseRelation = new Item[] {};
            Brackets = new[] {bracketPairItem};
        }

        public static PrioTable operator +(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        public static bool operator ==(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return ReferenceEquals(y, null);
            return x.Equals(y);
        }

        /// <summary>
        ///     Returns the priority information of a pair of tokens
        ///     The characters have the following meaning:
        ///     Plus: New token is higher the recent token,
        ///     Minus: Recent token is higher than new token
        ///     Equal sign: New token and recent token are matching
        /// </summary>
        /// <param name="newItem"> </param>
        /// <param name="recentItem"> </param>
        /// <returns> </returns>
        public char Relation(IItem newItem, IItem recentItem)
        {
            var newToken = newItem.Token == "" ? EndOfText : newItem.Token;
            var recentToken = recentItem.Token == "" ? BeginOfText : recentItem.Token;
            var newType = Type(newToken);
            var recentType = Type(recentToken);
            var depthDelta = newItem.Context.Depth - recentItem.Context.Depth;
            var indexDelta = Index(recentToken) - Index(newToken);

            if(depthDelta == 1)
            {
                if(newType == TokenType.Left && recentType == TokenType.Left)
                    return '+';
                if(newType == TokenType.Normal && recentType == TokenType.Left)
                    return '+';
                if(newType == TokenType.Right && recentType == TokenType.Left && indexDelta == 0)
                    return '=';
                NotImplementedMethod(newItem, recentItem);
                return default(char);
            }

            if(depthDelta == 0)
            {
                switch(newType)
                {
                    case TokenType.Left:
                        switch(recentType)
                        {
                            case TokenType.Left:
                                NotImplementedMethod(newItem, recentItem);
                                if(indexDelta > 0)
                                    return '+';
                                NotImplementedMethod(newItem, recentItem);
                                return default(char);
                            case TokenType.Normal:
                                return '+';
                            case TokenType.Right:
                                NotImplementedMethod(newItem, recentItem);
                                return default(char);
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    case TokenType.Normal:
                        switch(recentType)
                        {
                            case TokenType.Left:
                                NotImplementedMethod(newItem, recentItem);
                                return '+';
                            case TokenType.Right:
                                NotImplementedMethod(newItem, recentItem);
                                return '-';
                            case TokenType.Normal:
                                if(indexDelta < 0)
                                    return '-';
                                if(indexDelta > 0)
                                    return '+';
                                return BaseRelation[Index(recentToken)].IsRight ? '+' : '-';
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    case TokenType.Right:
                        switch(recentType)
                        {
                            case TokenType.Normal:
                                return '-';
                            case TokenType.Left:
                                NotImplementedMethod(newItem, recentItem);
                                return default(char);
                            case TokenType.Right:
                                NotImplementedMethod(newItem, recentItem);
                                if(indexDelta < 0)
                                    return '-';
                                NotImplementedMethod(newItem, recentItem);
                                return default(char);
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if(depthDelta == -1)
            {
                if(newType == TokenType.Normal && recentType == TokenType.Right)
                    return '-';
                if(newType == TokenType.Right && recentType == TokenType.Right && indexDelta < 0)
                    return '-';

                NotImplementedMethod(newItem, recentItem);
                return default(char);
            }

            NotImplementedMethod(newItem, recentItem);
            return default(char);
        }

        enum TokenType
        {
            Left,
            Normal,
            Right
        }

        TokenType Type(string token)
        {
            if(Brackets.Any(item => item.Left == token))
                return TokenType.Left;
            if(Brackets.Any(item => item.Right == token))
                return TokenType.Right;
            return TokenType.Normal;
        }

        int Index(string token)
        {
            return
                (Brackets.IndexWhere(item => item.Left == token || item.Right == token)
                    ?? BaseRelation.IndexWhere(item => item.Token.Contains(token))
                        ?? BaseRelation.IndexWhere(item => item.Token.Contains(Any))).AssertValue();
        }

        public Context NextContext(IItem current)
            => current.Context.Add(GetContextIndex(current.Token));

        public static Context StartContext => Context.Empty;

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
    }
}