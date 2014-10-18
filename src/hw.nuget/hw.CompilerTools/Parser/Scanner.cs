using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    abstract class Scanner : Dumpable
    {
        protected abstract int WhiteSpace(SourcePosn sourcePosn);
        protected abstract int? Number(SourcePosn sourcePosn);
        protected abstract int? Text(SourcePosn sourcePosn);
        protected abstract int? Any(SourcePosn sourcePosn);

        internal Item<IParsedSyntax> CreateToken
            (SourcePosn sourcePosn, ITokenFactory tokenFactory, Stack<OpenItem<IParsedSyntax>> stack)
        {
            var item = InternalCreateToken(sourcePosn, tokenFactory);
            var type = item.Type;
            var rescannable = type as IRescannable<IParsedSyntax>;
            if(rescannable != null)
                return rescannable.Execute(item.Part, sourcePosn, stack);

            return new Item<IParsedSyntax>((IType<IParsedSyntax>) type, item.Part);
        }

        RawItem InternalCreateToken
            (SourcePosn sourcePosn, ITokenFactory tokenFactory)
        {
            try
            {
                sourcePosn.Position += WhiteSpace(sourcePosn);
                return CreateAndAdvance(sourcePosn, sp => sp.IsEnd ? (int?)0 : null, tokenFactory.EndOfText)
                    ?? CreateAndAdvance(sourcePosn, Number, tokenFactory.Number)
                        ?? CreateAndAdvance(sourcePosn, Text, tokenFactory.Text)
                            ?? CreateAndAdvance(sourcePosn, Any, tokenFactory.TokenClass)
                                ?? WillReturnNull(sourcePosn);
            }
            catch (Exception exception)
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
            public readonly IType TokenClass;
            public readonly int Length;

            public Exception(SourcePosn sourcePosn, IType tokenClass, int length)
            {
                SourcePosn = sourcePosn;
                TokenClass = tokenClass;
                Length = length;
            }
        }

        static RawItem CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, IType tokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass);
        }
        static RawItem CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, IType> getTokenClass)
        {
            return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l)));
        }

        static RawItem CreateAndAdvance
            (SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<SourcePosn, int, IType> getTokenClass)
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