using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    /// <summary>
    ///     Language scanner, that returns groups of tokens at each call to <see cref="IScanner.GetNextTokenGroup" />.
    ///     A tokengroup in this implementation consists of a number of whitespace tokens and one actual token.
    ///     The token building is defined by a <see cref="ITokenFactory" />. <br />
    ///     This is the recommented scanner implementation
    /// </summary>
    /// <remarks>
    ///     - The scanning process will try to match any of the <see cref="ITokenFactory.Classes" /> in order provided.
    ///     If that fails, the next character will be returned with token type
    ///     <see cref="ITokenFactory.InvalidCharacterError" />.<br />
    ///     - Only the last (or only) token in group returned <see cref="IScanner.GetNextTokenGroup" /> by will have a token
    ///     class with <see cref="IParserTokenFactory" /> defined.<br />
    ///     - During scanning process, exceptions of type <see cref="TwoLayerScanner.Exception" /> will be catched
    ///     and converted into appropriate token<br />
    ///     - When the <see cref="ITokenFactory.EndOfText" /> is returned the source position is set to invalid.<br />
    /// </remarks>
    public sealed class TwoLayerScanner : Dumpable, IScanner
    {
        sealed class Worker : DumpableObject
        {
            sealed class Item : DumpableObject, IItem
            {
                readonly SourcePart SourcePart;
                readonly IScannerTokenType Type;

                public Item(SourcePart sourcePart, IScannerTokenType type)
                {
                    SourcePart = sourcePart;
                    Type = type;
                }

                IScannerTokenType IItem.ScannerTokenType => Type;
                SourcePart IItem.SourcePart => SourcePart;

                protected override string GetNodeDump() {return base.GetNodeDump()+"("+Type.Id+")";}

            }

            readonly TwoLayerScanner Parent;
            readonly SourcePosn SourcePosn;

            internal Worker(TwoLayerScanner parent, SourcePosn sourcePosn)
            {
                Tracer.Assert(sourcePosn.IsValid);
                Parent = parent;
                SourcePosn = sourcePosn;
            }

            ITokenFactory TokenFactory => Parent.TokenFactory;

            internal IEnumerable<IItem> GetNextTokenGroup()
            {
                while(true)
                {
                    var t = GetNextToken();
                    yield return t;

                    if(t.ScannerTokenType.ParserTokenFactory != null)
                        yield break;
                }
            }

            IItem GetNextToken()
            {
                try
                {
                    if(SourcePosn.IsEnd)
                        return CreateAndAdvance(length: 0, type: TokenFactory.EndOfText);

                    foreach(var item in TokenFactory.Classes)
                    {
                        var length = item.Match(SourcePosn);
                        if(length != null)
                            return CreateAndAdvance(length.Value, item.ScannerTokenType);
                    }

                    return CreateAndAdvance(length: 1, type: TokenFactory.InvalidCharacterError);
                }
                catch(Exception scannerException)
                {
                    return CreateAndAdvance
                        (scannerException.SourcePosn - SourcePosn, scannerException.SyntaxError);
                }
            }

            void Advance(int position) => Advance(SourcePosn, position);

            static void Advance(SourcePosn sourcePosn, int position)
            {
                var wasEnd = sourcePosn.IsEnd;
                sourcePosn.Position += position;
                if(wasEnd && position > 0)
                    sourcePosn.IsValid = false;
            }

            IItem CreateAndAdvance(int length, IScannerTokenType type)
            {
                var result = new Item(SourcePart.Span(SourcePosn, length), type);
                Advance(length);
                return result;
            }
        }

        /// <summary>
        ///     Exception type that is requognized by <see cref="TwoLayerScanner" /> processing
        ///     and used to convert it into a correct token,
        ///     containing the <see cref="IScannerTokenType" /> provided with exception
        ///     to identify the error on higher level.
        /// </summary>
        public sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly IScannerTokenType SyntaxError;

            public Exception(SourcePosn sourcePosn, IScannerTokenType syntaxError)
            {
                SourcePosn = sourcePosn;
                SyntaxError = syntaxError;
            }
        }

        readonly ITokenFactory TokenFactory;
        public TwoLayerScanner(ITokenFactory tokenFactory) { TokenFactory = tokenFactory; }

        IItem[] IScanner.GetNextTokenGroup(SourcePosn sourcePosn)
            => new Worker(this, sourcePosn).GetNextTokenGroup().ToArray();
    }
}