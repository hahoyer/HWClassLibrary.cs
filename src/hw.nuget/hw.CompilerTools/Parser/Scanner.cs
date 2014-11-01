using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public class Scanner<TTreeItem> : Dumpable, IScanner<TTreeItem>
        where TTreeItem : class
    {
        readonly ILexer _lexer;

        public Scanner(ILexer lexer) { _lexer = lexer; }

        int WhiteSpace(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.WhiteSpace); }
        int? Number(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Number); }
        int? Text(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Text); }
        int? Any(SourcePosn sourcePosn) { return ExceptionGuard(sourcePosn, _lexer.Any); }

        ScannerItem<TTreeItem> IScanner<TTreeItem>.NextToken
            (SourcePosn sourcePosn, ITokenFactory<TTreeItem> tokenFactory, Stack<OpenItem<TTreeItem>> stack)
        {
            return NextToken(sourcePosn, tokenFactory);
        }

        ScannerItem<TTreeItem> NextToken(SourcePosn sourcePosn, ITokenFactory<TTreeItem> tokenFactory)
        {
            try
            {
                Tracer.Assert(sourcePosn.IsValid);
                sourcePosn.Position += WhiteSpace(sourcePosn);
                return CreateAndAdvance(sourcePosn, sp => sp.IsEnd ? (int?) 0 : null, tokenFactory.EndOfText)
                    ?? CreateAndAdvance(sourcePosn, Number, tokenFactory.Number)
                        ?? CreateAndAdvance(sourcePosn, Text, tokenFactory.Text)
                            ?? CreateAndAdvance(sourcePosn, Any, tokenFactory.TokenClass)
                                ?? WillReturnNull(sourcePosn);
            }
            catch(Exception exception)
            {
                return CreateAndAdvance(exception.SourcePosn, sp => exception.Length, tokenFactory.Error(exception.Error));
            }
        }

        ScannerItem<TTreeItem> WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
        }

        internal sealed class Exception : System.Exception
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

        static ScannerItem<TTreeItem> CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, IType<TTreeItem> tokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass);
        }
        static ScannerItem<TTreeItem> CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, IType<TTreeItem>> getTokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l)));
        }

        static ScannerItem<TTreeItem> CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<SourcePosn, int, IType<TTreeItem>> getTokenClass)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new ScannerItem<TTreeItem>
                (getTokenClass(sourcePosn, length.Value), SourcePart.Span(sourcePosn, length.Value));
            var wasEnd = sourcePosn.IsEnd;
            sourcePosn.Position += length.Value;
            if(wasEnd)
                sourcePosn.IsValid = false;
            return result;
        }

        TResult ExceptionGuard<TResult>(SourcePosn sourcePosn, Func<SourcePosn, TResult> match)
        {
            try
            {
                return match(sourcePosn);
            }
            catch(Match.Exception exception)
            {
                throw new Exception(sourcePosn, sourcePosn - exception.SourcePosn, exception.Error);
            }
        }
    }
}