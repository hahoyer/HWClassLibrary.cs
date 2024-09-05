﻿using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser;

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

    public sealed class RelationDefinitionItem(bool isRight, string[] token)
    {
        public readonly string[] Token = token;
        internal readonly bool IsRight = isRight;
    }

    public sealed class BracketPairItem(string left, string right)
    {
        public readonly string Left = left;
        public readonly string Right = right;
    }

    public sealed class Relation : EnumEx
    {
        public static readonly Relation Push = new() { IsPush = true };

        public static readonly Relation Pull = new() { IsPull = true };

        public static readonly Relation Match = new()
        {
            IsPull = true, IsBracket = true, IsMatch = true
        };

        public static readonly Relation Mismatch = new()
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
    [
        [FunctionType.Push, FunctionType.Push, FunctionType.Push]
        , [FunctionType.Push, FunctionType.Relation, FunctionType.Relation]
        , [FunctionType.Match, FunctionType.Pull, FunctionType.Pull]
    ];

    public string Title;

    readonly RelationDefinitionItem[] BaseRelation;
    readonly BracketPairItem[] Brackets;
    BracketContext BracketContextValue;

    [DisableDump]
    public BracketContext BracketContext
        => BracketContextValue ??= BracketContext.Start(Brackets);

    PrioTable()
    {
        BaseRelation = [];
        Brackets = [];
    }

    PrioTable(bool isRight, string[] token)
        : this() => BaseRelation = [new(isRight, token)];

    PrioTable(PrioTable target, PrioTable y)
        : this()
    {
        BaseRelation = target.BaseRelation.Concat(y.BaseRelation).ToArray();
        Brackets = target.Brackets.Concat(y.Brackets).ToArray();
    }

    PrioTable(BracketPairItem bracketPairItem)
        : this() => Brackets = [bracketPairItem];

    public override bool Equals(object obj)
    {
        var target = obj as PrioTable;
        if(target == null)
            return false;
        return BaseRelation == target.BaseRelation && Brackets == target.Brackets;
    }

    public override int GetHashCode() => BaseRelation.GetHashCode() + Brackets.GetHashCode();

    public override string ToString() => Title ?? Dump();

    /// <summary>
    ///     Define a prio table with tokens that have the same priority and are left associative
    /// </summary>
    /// <param name="target"> </param>
    /// <returns> </returns>
    public static PrioTable Left(params string[] target) => new(false, target);

    /// <summary>
    ///     Define a prio table with tokens that have the same priority and are right associative
    /// </summary>
    /// <param name="target"> </param>
    /// <returns> </returns>
    public static PrioTable Right(params string[] target) => new(true, target);

    public static PrioTable BracketParallels(string[] leftBrackets, string[] rightBrackets)
    {
        (leftBrackets.Length == rightBrackets.Length).Assert();
        return leftBrackets
            .Select((item, index) => BracketPair(item, rightBrackets[index]))
            .Aggregate(new PrioTable(), (c, n) => new(c, n));
    }

    public static PrioTable BracketPair(string left, string right) => new(new(left, right));

    public static PrioTable FromText(string text) => FromText(text.Split('\n', '\r'));

    public static PrioTable operator +(PrioTable target, PrioTable y) => new(target, y);

    public static bool operator ==(PrioTable target, PrioTable y)
        => target?.Equals(y) ?? ReferenceEquals(y, null);

    public static bool operator !=(PrioTable target, PrioTable y)
        => !target?.Equals(y) ?? !ReferenceEquals(y, null);

    public Relation GetRelation(ITargetItem newItem, ITargetItem recentItem)
    {
        var delta = newItem.LeftContext.Depth - BracketContext.GetRightDepth(recentItem);

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

        result += BracketParallels([BeginOfText], [EndOfText]);
        return result;
    }

    int? BaseRelationIndex(string token) => BaseRelation.IndexWhere(item => item.Token.Contains(token));

    int GetSubTypeIndex(ITargetItem target)
    {
        var parent = this;
        (!string.IsNullOrWhiteSpace(target.Token)).Assert();
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
        => SameDepth[newIndex][otherIndex] switch
        {
            FunctionType.Unknown => NotImplemented(newToken, otherToken)
            , FunctionType.Push => Relation.Push
            , FunctionType.Relation => GetRelation(newIndex, otherIndex, newToken, otherToken)
            , FunctionType.Pull => Relation.Pull
            , FunctionType.Match => GetBracketMatch(newToken, otherToken)
            , var _ => throw new ArgumentOutOfRangeException()
        };

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
        switch(type)
        {
            case 0:
                result ??= BaseRelationIndex(LeftBracket);
                break;
            case 2:
                result ??= BaseRelationIndex(RightBracket);
                break;
        }

        if(type != 1)
            result ??= BaseRelationIndex(Bracket);
        result ??= BaseRelationIndex(Any);
        return result.AssertValue();
    }
}