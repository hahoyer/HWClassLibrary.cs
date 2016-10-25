using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    public class Match2TwoLayerScannerGuard : DumpableObject
    {
        readonly Func<Match.IError, IScannerTokenType> Convert;

        public Match2TwoLayerScannerGuard(Func<Match.IError, IScannerTokenType> convert)
        {
            Convert = convert;
        }

        public int? GuardedMatch(SourcePosn sourcePosn, IMatch match)
        {
            try
            {
                return sourcePosn.Match(match);
            }
            catch(Match.Exception exception)
            {
                throw new TwoLayerScanner.Exception
                (
                    exception.SourcePosn,
                    Convert(exception.Error)
                );
            }
        }
    }
}