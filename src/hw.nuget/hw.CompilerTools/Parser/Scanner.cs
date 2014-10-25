using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Token
    {
        public readonly IType<ParsedSyntax> Type;
        public readonly SourcePart Part;

        public Token(IType<ParsedSyntax> type, SourcePart part)
        {
            Type = type;
            Part = part;
        }
    }


    public abstract class Scanner : Dumpable, IScanner<ParsedSyntax>
    {
        protected abstract int WhiteSpace(SourcePosn sourcePosn);
        protected abstract int? Number(SourcePosn sourcePosn);
        protected abstract int? Text(SourcePosn sourcePosn);
        protected abstract int? Any(SourcePosn sourcePosn);

        Token IScanner<ParsedSyntax>.NextToken(SourcePosn sourcePosn, ITokenFactory<ParsedSyntax> tokenFactory, Stack<OpenItem<ParsedSyntax>> stack)
        {
            return NextToken(sourcePosn, tokenFactory);
        }

        Token NextToken(SourcePosn sourcePosn, ITokenFactory<ParsedSyntax> tokenFactory)
        {
            try
            {
                sourcePosn.Position += WhiteSpace(sourcePosn);
                return CreateAndAdvance(sourcePosn, sp => sp.IsEnd ? (int?) 0 : null, tokenFactory.EndOfText)
                    ?? CreateAndAdvance(sourcePosn, Number, tokenFactory.Number)
                        ?? CreateAndAdvance(sourcePosn, Text, tokenFactory.Text)
                            ?? CreateAndAdvance(sourcePosn, Any, tokenFactory.TokenClass)
                                ?? WillReturnNull(sourcePosn);
            }
            catch(Exception exception)
            {
                return CreateAndAdvance(exception.SourcePosn, sp => exception.Length, exception.TokenClass);
            }
        }

        Token WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
        }

        internal sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly IType<ParsedSyntax> TokenClass;
            public readonly int Length;

            public Exception(SourcePosn sourcePosn, IType<ParsedSyntax> tokenClass, int length)
            {
                SourcePosn = sourcePosn;
                TokenClass = tokenClass;
                Length = length;
            }
        }

        static Token CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, IType<ParsedSyntax> tokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass);
        }
        static Token CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, IType<ParsedSyntax>> getTokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l)));
        }

        static Token CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<SourcePosn, int, IType<ParsedSyntax>> getTokenClass)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new Token
                (getTokenClass(sourcePosn, length.Value), SourcePart.Span(sourcePosn, length.Value));
            sourcePosn.Position += length.Value;
            return result;
        }
    }
}