using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    sealed class ScannerToken
    {
        public readonly WhiteSpaceToken[] PrecededWith;
        public readonly SourcePart Characters;

        public ScannerToken(SourcePart characters, WhiteSpaceToken[] precededWith)
        {
            Characters = characters;
            PrecededWith = precededWith ?? new WhiteSpaceToken[0];
            AssertValid();
        }

        void AssertValid()
        {
            for(var i = 1; i < PrecededWith.Length; i++)
                Tracer.Assert
                    (PrecededWith[i - 1].Characters.End.Equals(PrecededWith[i].Characters.Start));
            var l = PrecededWith.LastOrDefault();
            if(l == null)
                return;
            Tracer.Assert(l.Characters.End.Equals(Characters.Start));
        }

        public SourcePart SourcePart { get { return (Characters + PrecededWith.SourcePart()); } }

        [UsedImplicitly]
        public string Id { get { return Characters.Id; } }

        [UsedImplicitly]
        public string NodeDump { get { return Id; } }
    }
}