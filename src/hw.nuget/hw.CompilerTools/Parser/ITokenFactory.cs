using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface ITokenFactory
    {
        IScannerTokenType EndOfText { get; }
        IScannerTokenType InvalidCharacterError { get; }
        LexerItem[] Classes { get; }
    }
}