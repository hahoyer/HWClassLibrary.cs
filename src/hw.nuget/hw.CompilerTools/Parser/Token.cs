using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    public sealed class Token
    {
        public readonly WhiteSpaceToken[] PrecededWith;
        public readonly SourcePart Characters;

        public Token(SourcePart characters, WhiteSpaceToken[] precededWith)
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

        public SourcePosn Start { get { return SourcePart.Start; } }
        public SourcePart SourcePart
        {
            get { return (Characters + PrecededWith.Select(item => item.Characters).Aggregate()); }
        }

        public string Name { get { return Characters.Name; } }

        [UsedImplicitly]
        public string NodeDump { get { return Name; } }
    }
}