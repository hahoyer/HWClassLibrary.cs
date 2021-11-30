using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    /// <summary>
    ///     Language scanner, that returns groups of tokens at each call to <see cref="IScanner.GetNextTokenGroup" />.
    ///     A token-group in this implementation consists of a number of whitespace tokens and one actual token.
    ///     The token building is defined by a <see cref="ITokenFactory" />. <br />
    ///     This is the recommended scanner implementation
    /// </summary>
    /// <remarks>
    ///     - The scanning process will try to match any of the <see cref="ITokenFactory.Classes" /> in order provided.
    ///     If that fails, the next character will be returned with token type
    ///     <see cref="ITokenFactory.InvalidCharacterError" />.<br />
    ///     - Only the last (or only) token in group returned <see cref="IScanner.GetNextTokenGroup" /> by will have a token
    ///     class with <see cref="IParserTokenFactory" /> defined.<br />
    ///     - During scanning process, exceptions of type <see cref="TwoLayerScanner.Exception" /> will be catch-ed
    ///     and converted into appropriate token<br />
    ///     - When the <see cref="ITokenFactory.EndOfText" /> is returned the source position is set to invalid.<br />
    /// </remarks>
    public sealed class TwoLayerScanner
        : Dumpable
            , IScanner
    {
        /// <summary>
        ///     Exception type that is recognized by <see cref="TwoLayerScanner" /> processing
        ///     and used to convert it into a correct token,
        ///     containing the <see cref="IScannerTokenType" /> provided with exception
        ///     to identify the error on higher level.
        /// </summary>
        public sealed class Exception : System.Exception
        {
            public readonly SourcePosition SourcePosition;
            public readonly IScannerTokenType SyntaxError;

            public Exception(SourcePosition sourcePosition, IScannerTokenType syntaxError)
            {
                SourcePosition = sourcePosition;
                SyntaxError = syntaxError;
            }
        }

        sealed class Worker : DumpableObject
        {
            sealed class Item
                : DumpableObject
                    , IItem
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

                protected override string GetNodeDump() => base.GetNodeDump() + "(" + Type.Id + ")";
            }

            readonly TwoLayerScanner Parent;
            readonly SourcePosition SourcePosition;

            internal Worker(TwoLayerScanner parent, SourcePosition sourcePosition)
            {
                sourcePosition.IsValid.Assert();
                Parent = parent;
                SourcePosition = sourcePosition;
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
                    if(SourcePosition.IsEnd)
                        return CreateAndAdvance(0, TokenFactory.EndOfText);

                    foreach(var item in TokenFactory.Classes)
                    {
                        var length = item.Match(SourcePosition);
                        if(length != null)
                            return CreateAndAdvance(length.Value, item.ScannerTokenType);
                    }

                    return CreateAndAdvance(1, TokenFactory.InvalidCharacterError);
                }
                catch(Exception scannerException)
                {
                    return CreateAndAdvance
                        (scannerException.SourcePosition - SourcePosition, scannerException.SyntaxError);
                }
            }

            void Advance(int position) => Advance(SourcePosition, position);

            static void Advance(SourcePosition sourcePosition, int position)
            {
                var wasEnd = sourcePosition.IsEnd;
                sourcePosition.Position += position;
                if(wasEnd && position > 0)
                    sourcePosition.IsValid = false;
            }

            IItem CreateAndAdvance(int length, IScannerTokenType type)
            {
                var result = new Item(SourcePart.Span(SourcePosition, length), type);
                Advance(length);
                return result;
            }
        }

        readonly ITokenFactory TokenFactory;
        public TwoLayerScanner(ITokenFactory tokenFactory) => TokenFactory = tokenFactory;

        IItem[] IScanner.GetNextTokenGroup(SourcePosition sourcePosition)
            => new Worker(this, sourcePosition).GetNextTokenGroup().ToArray();
    }
}