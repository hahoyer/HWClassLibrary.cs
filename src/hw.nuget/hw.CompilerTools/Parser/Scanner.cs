using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Scanner : Dumpable, IScanner
    {
        readonly ITokenFactory TokenFactory;
        public Scanner(ITokenFactory tokenFactory) { TokenFactory = tokenFactory; }

        IItem[] IScanner.GetNextTokenGroup(SourcePosn sourcePosn)
            => new Worker(this, sourcePosn).GetNextTokenGroup().ToArray();

        sealed class Worker : DumpableObject
        {
            readonly Scanner Parent;
            readonly SourcePosn SourcePosn;

            internal Worker(Scanner parent, SourcePosn sourcePosn)
            {
                Tracer.Assert(sourcePosn.IsValid);
                Parent = parent;
                SourcePosn = sourcePosn;
            }

            internal IEnumerable<IItem> GetNextTokenGroup()
            {
                while(true)
                {
                    var t = GetNextToken();
                    yield return t;

                    if(t.ScannerType is IParserTypeProvider)
                        yield break;
                }
            }

            IItem GetNextToken()
            {
                try
                {
                    if(SourcePosn.IsEnd)
                        return CreateAndAdvance(0, TokenFactory.EndOfText);

                    foreach(var item in TokenFactory.Classes)
                    {
                        var length = item.Match(SourcePosn);
                        if(length != null)
                            return CreateAndAdvance(length.Value, item.ScannerType);
                    }

                    return CreateAndAdvance(1, TokenFactory.InvalidCharacterError);
                }
                catch(Exception exception)
                {
                    var scannerException = exception as IException;
                    if(scannerException == null)
                        throw;

                    return CreateAndAdvance(scannerException.Length, scannerException.SyntaxError);
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

            ITokenFactory TokenFactory => Parent.TokenFactory;

            IItem CreateAndAdvance(int length, IScannerType type)
            {
                var result = new Item(SourcePart.Span(SourcePosn, length), type);
                Advance(length);
                return result;
            }

            sealed class Item : DumpableObject, IItem
            {
                readonly SourcePart SourcePart;
                readonly IScannerType Type;

                public Item(SourcePart sourcePart, IScannerType type)
                {
                    SourcePart = sourcePart;
                    Type = type;
                }

                IScannerType IItem.ScannerType => Type;
                SourcePart IItem.SourcePart => SourcePart;
            }
        }

        internal interface IException
        {
            int Length { get; }
            IScannerType SyntaxError { get; }
        }
    }
}