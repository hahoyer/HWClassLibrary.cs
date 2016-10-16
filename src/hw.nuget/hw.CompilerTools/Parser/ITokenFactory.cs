using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Parser
{
    public interface ITokenFactory
    {
        IScannerType EndOfText { get; }
        IScannerType InvalidCharacterError { get; }
        LexerItem[] Classes { get; }
    }
}