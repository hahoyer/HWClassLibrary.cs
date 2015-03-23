using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Scanner<TTreeItem> : Dumpable, IScanner<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        readonly ILexer _lexer;
        readonly ITokenFactory<TTreeItem> _tokenFactory;

        public Scanner
            (
            ILexer lexer,
            ITokenFactory<TTreeItem> tokenFactory)
        {
            _lexer = lexer;
            _tokenFactory = tokenFactory;
        }

        WhiteSpaceToken[] GuardedWhiteSpace(SourcePosn sourcePosn)
        {
            return ExceptionGuard(sourcePosn, posn => WhiteSpace(posn).ToArray());
        }

        int? Number(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Number); }
        int? Text(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Text); }
        int? Any(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Any); }

        Item IScanner<TTreeItem>.NextToken(SourcePosn sourcePosn) { return NextToken(sourcePosn); }

        Item NextToken(SourcePosn sourcePosn)
        {
            var preceededBy = new WhiteSpaceToken[0];
            try
            {
                Tracer.Assert(sourcePosn.IsValid);
                preceededBy = GuardedWhiteSpace(sourcePosn);
                sourcePosn.Position += preceededBy.Sum(item => item.Characters.Length);
                return CreateAndAdvance
                    (
                        sourcePosn,
                        sp => sp.IsEnd ? (int?) 0 : null,
                        () => _tokenFactory.EndOfText,
                        preceededBy)
                    ?? CreateAndAdvance(sourcePosn, Number, () => _tokenFactory.Number, preceededBy)
                        ?? CreateAndAdvance(sourcePosn, Text, () => _tokenFactory.Text, preceededBy)
                            ?? CreateAndAdvance
                                (sourcePosn, Any, _tokenFactory.TokenClass, preceededBy)
                                ?? WillReturnNull(sourcePosn);
            }
            catch(Exception exception)
            {
                return CreateAndAdvance
                    (
                        exception.SourcePosn,
                        sp => exception.Length,
                        () => _tokenFactory.Error(exception.Error),
                        preceededBy);
            }
        }

        IEnumerable<WhiteSpaceToken> WhiteSpace(SourcePosn original)
        {
            var current = original.Clone;
            var runAgain = true;
            while(runAgain)
            {
                runAgain = false;

                var result = _lexer
                    .WhiteSpace
                    .SelectMany((f, i) => CreateAndAdvance(current, f, i).NullableToArray())
                    .ToArray();

                foreach(var token in result)
                {
                    yield return token;
                    runAgain = true;
                }
            }
        }

        static WhiteSpaceToken CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, int index)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new WhiteSpaceToken(index, SourcePart.Span(sourcePosn, length.Value));
            var wasEnd = sourcePosn.IsEnd;
            sourcePosn.Position += length.Value;
            if(wasEnd)
                sourcePosn.IsValid = false;
            return result;
        }


        Item WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
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

        static Item CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<IType> getTokenClass,
            WhiteSpaceToken[] preceededBy)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(), preceededBy);
        }
        static Item CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<string, IType> getTokenClass,
            WhiteSpaceToken[] preceededBy)
        {
            return CreateAndAdvance
                (sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l)), preceededBy);
        }

        static Item CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<SourcePosn, int, IType> getTokenClass,
            WhiteSpaceToken[] preceededBy)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new Item
                (
                getTokenClass(sourcePosn, length.Value),
                new ScannerToken(SourcePart.Span(sourcePosn, length.Value), preceededBy)
                );
            var wasEnd = sourcePosn.IsEnd;
            sourcePosn.Position += length.Value;
            if(wasEnd)
                sourcePosn.IsValid = false;
            return result;
        }

        static TResult ExceptionGuard<TResult>
            (SourcePosn sourcePosn, Func<SourcePosn, TResult> match)
        {
            try
            {
                return match(sourcePosn);
            }
            catch(Match.Exception exception)
            {
                throw new Exception(sourcePosn, exception.SourcePosn - sourcePosn, exception.Error);
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