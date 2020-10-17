using System;
using hw.DebugFormatter;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    [PublicAPI]
    public class Match2TwoLayerScannerGuard : DumpableObject
    {
        readonly Func<Match.IError, IScannerTokenType> Convert;

        public Match2TwoLayerScannerGuard(Func<Match.IError, IScannerTokenType> convert) => Convert = convert;

        public int? GuardedMatch(SourcePosition sourcePosition, IMatch match)
        {
            try
            {
                return sourcePosition.Match(match);
            }
            catch(Match.Exception exception)
            {
                throw new TwoLayerScanner.Exception
                (
                    exception.SourcePosition,
                    Convert(exception.Error)
                );
            }
        }
    }
}