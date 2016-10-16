using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    sealed class LexerException : Exception, Parser.Scanner.IException
    {
        readonly SourcePosn SourcePosn;
        readonly TokenFactory.SyntaxError SyntaxError;

        public LexerException(SourcePosn sourcePosn, TokenFactory.SyntaxError syntaxError)
        {
            SourcePosn = sourcePosn;
            SyntaxError = syntaxError;
        }

        SourcePosn Parser.Scanner.IException.SourcePosn => SourcePosn;
        IScannerType Parser.Scanner.IException.SyntaxError => SyntaxError;
    }
}