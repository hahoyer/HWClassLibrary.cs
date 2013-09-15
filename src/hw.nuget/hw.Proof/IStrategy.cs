using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;

namespace Reni.Proof
{
    internal interface IStrategy
    {
        Set<ParsedSyntax> Apply(ParsedSyntax parsedSyntax);
    }
}