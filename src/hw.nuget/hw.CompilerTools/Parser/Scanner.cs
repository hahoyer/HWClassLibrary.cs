using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    public interface ISubParser
    {
        Item<IParsedSyntax, TokenData> Execute
            (TokenData part, SourcePosn sourcePosn, Stack<OpenItem<IParsedSyntax, TokenData>> stack);
    }

    public sealed class RawItem
    {
        public readonly IType<IParsedSyntax, TokenData> Type;
        public readonly TokenData Part;

        public RawItem(IType<IParsedSyntax, TokenData> type, TokenData part)
        {
            Type = type;
            Part = part;
        }

        public Item<IParsedSyntax, TokenData> Finalize() { return new Item<IParsedSyntax, TokenData>(Type, Part); }
    }


    abstract class Scanner : Dumpable
    {
        protected abstract int WhiteSpace(SourcePosn sourcePosn);
        protected abstract int? Number(SourcePosn sourcePosn);
        protected abstract int? Text(SourcePosn sourcePosn);
        protected abstract int? Any(SourcePosn sourcePosn);

        internal Item<IParsedSyntax, TokenData> CreateToken
            (
            SourcePosn sourcePosn,
            ITokenFactory<IParsedSyntax, TokenData> tokenFactory,
            Stack<OpenItem<IParsedSyntax, TokenData>> stack)
        {
            var item = InternalCreateToken(sourcePosn, tokenFactory);
            var rescannable = item.Type as ISubParser;
            return rescannable == null ? item.Finalize() : rescannable.Execute(item.Part, sourcePosn, stack);
        }

        RawItem InternalCreateToken
            (SourcePosn sourcePosn, ITokenFactory<IParsedSyntax, TokenData> tokenFactory)
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

        RawItem WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
        }

        internal sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly IType<IParsedSyntax, TokenData> TokenClass;
            public readonly int Length;

            public Exception(SourcePosn sourcePosn, IType<IParsedSyntax, TokenData> tokenClass, int length)
            {
                SourcePosn = sourcePosn;
                TokenClass = tokenClass;
                Length = length;
            }
        }

        static RawItem CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, IType<IParsedSyntax, TokenData> tokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass);
        }
        static RawItem CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, IType<IParsedSyntax, TokenData>> getTokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l)));
        }

        static RawItem CreateAndAdvance
            (
            SourcePosn sourcePosn,
            Func<SourcePosn, int?> getLength,
            Func<SourcePosn, int, IType<IParsedSyntax, TokenData>> getTokenClass)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new RawItem
                (getTokenClass(sourcePosn, length.Value), TokenData.Span(sourcePosn, length.Value));
            sourcePosn.Position += length.Value;
            return result;
        }
    }
}