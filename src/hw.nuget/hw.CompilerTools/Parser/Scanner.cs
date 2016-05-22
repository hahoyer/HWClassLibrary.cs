using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Scanner<TTreeItem> : Dumpable, IScanner<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        readonly ILexer Lexer;
        readonly ITokenFactory<TTreeItem> TokenFactory;

        public Scanner(ILexer lexer, ITokenFactory<TTreeItem> tokenFactory)
        {
            Lexer = lexer;
            TokenFactory = tokenFactory;
        }

        Item IScanner<TTreeItem>.NextToken(SourcePosn sourcePosn)
            => new Worker(this, sourcePosn).GetNextToken();

        IType InvalidCharacterError => TokenFactory.Error(Lexer.InvalidCharacterError);

        sealed class Worker : DumpableObject
        {
            readonly Scanner<TTreeItem> Parent;
            readonly SourcePosn SourcePosn;
            readonly WhiteSpaceToken[] PreceededBy;

            internal Worker(Scanner<TTreeItem> parent, SourcePosn sourcePosn)
            {
                Tracer.Assert(sourcePosn.IsValid);
                Parent = parent;
                SourcePosn = sourcePosn;
                PreceededBy = GuardedGetPreceededBy();
            }

            WhiteSpaceToken[] GuardedGetPreceededBy()
                => ExceptionGuard(posn => GetPreceededBy().ToArray());

            IEnumerable<WhiteSpaceToken> GetPreceededBy()
            {
                var runAgain = true;
                while(runAgain)
                {
                    runAgain = false;

                    var result = Lexer
                        .WhiteSpace
                        .SelectMany
                        (
                            (getMatch, index) =>
                                GetNextWhiteSpaceToken
                                    (index, ExceptionGuard(() => getMatch(SourcePosn)))
                                    .NullableToArray()
                        )
                        .ToArray();

                    foreach(var token in result)
                    {
                        yield return token;

                        runAgain = true;
                    }
                }
            }


            Item InvalidCharacter()
            {
                // this statement will throw an exception in case of error in whitespace
                var result = Lexer
                    .WhiteSpace
                    .All(f => ExceptionGuard(f) == null);
                // otherwise current character will not be any whitespace
                Tracer.Assert(result);
                // and is considered as invalid
                return CreateAndAdvance(1, () => Parent.InvalidCharacterError);
            }

            WhiteSpaceToken GetNextWhiteSpaceToken(int index, int? length)
            {
                if(length == null)
                    return null;

                var sourcePart = SourcePart.Span(SourcePosn, length.Value);
                Advance(length.Value);
                return new WhiteSpaceToken(index, sourcePart);
            }

            internal Item GetNextToken()
            {
                try
                {
                    var endMarker = SourcePosn.IsEnd ? (int?) 0 : null;
                    return CreateAndAdvance(endMarker, () => TokenFactory.EndOfText)
                        ?? CreateAndAdvance(Number(), () => TokenFactory.Number)
                            ?? CreateAndAdvance(Text(), () => TokenFactory.Text)
                                ?? CreateAndAdvance(Any(), TokenFactory.TokenClass)
                                    ?? InvalidCharacter();
                }
                catch(Exception exception)
                {
                    Func<IType> getTokenClass = () => TokenFactory.Error(exception.Error);
                    return CreateAndAdvance(exception.Length, getTokenClass);
                }
            }

            void Advance(int position) => Advance(SourcePosn, position);

            static void Advance(SourcePosn sourcePosn, int position)
            {
                var wasEnd = sourcePosn.IsEnd;
                sourcePosn.Position += position;
                if(wasEnd)
                    sourcePosn.IsValid = false;
            }

            ITokenFactory<TTreeItem> TokenFactory => Parent.TokenFactory;
            ILexer Lexer => Parent.Lexer;
            int? Number() => ExceptionGuard(Lexer.Number);
            int? Text() => ExceptionGuard(Lexer.Text);
            int? Any() => ExceptionGuard(Lexer.Any);

            Item CreateAndAdvance(int? length, Func<IType> tokenClass)
                => CreateAndAdvance(length, (int l) => tokenClass());

            Item CreateAndAdvance(int? length, Func<string, IType> getTokenClass)
                => CreateAndAdvance(length, l => getTokenClass(SourcePosn.SubString(0, l)));

            Item CreateAndAdvance(int? length, Func<int, IType> getTokenClass)
            {
                if(length == null)
                    return null;

                var token = new ScannerToken(SourcePart.Span(SourcePosn, length.Value), PreceededBy);
                var result = new Item(getTokenClass(length.Value), token);
                Advance(length.Value);
                return result;
            }

            TResult ExceptionGuard<TResult>(Func<SourcePosn, TResult> match)
            {
                try
                {
                    return match(SourcePosn);
                }
                catch(Match.Exception exception)
                {
                    throw new Exception
                        (SourcePosn, exception.SourcePosn - SourcePosn, exception.Error);
                }
            }

            static int? ExceptionGuard(Func<int?> getLength)
            {
                try
                {
                    return getLength();
                }
                catch(Match.Exception)
                {
                    return null;
                }
            }
        }

        sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            internal readonly int Length;
            public readonly Match.IError Error;

            public Exception(SourcePosn sourcePosn, int length, Match.IError error)
            {
                SourcePosn = sourcePosn;
                Length = length;
                Error = error;
            }
        }


        public sealed class Item
        {
            internal readonly IType Type;
            internal readonly ScannerToken Token;

            internal Item(IType type, ScannerToken token)
            {
                Type = type;
                Token = token;
            }
        }

        public interface IType
        {
            ISubParser<TTreeItem> NextParser { get; }
            IType<TTreeItem> Type { get; }
        }
    }
}