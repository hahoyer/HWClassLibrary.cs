using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using JetBrains.Annotations;

namespace hw.Scanner
{
    /// <summary>
    ///     Source and position for compilation process
    /// </summary>
    [DebuggerDisplay("{NodeDump}")]
    sealed class SourcePosn : Dumpable
    {
        public readonly Source Source;
        public int Position;

        /// <summary>
        ///     ctor from source and position
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        public SourcePosn(Source source, int position)
        {
            Position = position;
            Source = source;
        }

        /// <summary>
        ///     The current character
        /// </summary>
        public char Current { get { return Source[Position]; } }

        /// <summary>
        ///     Natuaral indexer
        /// </summary>
        public char this[int index] { get { return Source[Position + index]; } }

        /// <summary>
        ///     Checks if at or beyond end of source
        /// </summary>
        /// <value> </value>
        public bool IsEnd { get { return Source.IsEnd(Position); } }

        /// <summary>
        ///     Obtains a piece
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length) { return Source.SubString(Position + start, length); }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): tag: " string</returns>
        public string FilePosn(string flagText) { return Source.FilePosn(Position, flagText); }

        /// <summary>
        ///     Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion) { return "\n" + FilePosn("see there"); }

        [UsedImplicitly]
        string DumpCurrent { get { return IsEnd ? "" : ("" + Current); } }

        const int DumpWidth = 10;

        [UsedImplicitly]
        string DumpAfterCurrent
        {
            get
            {
                if(IsEnd)
                    return "";
                var length = Math.Min(DumpWidth, Source.Length - Position - 1);
                var result = Source.SubString(Position + 1, length);
                if(length == DumpWidth)
                    result += "...";
                return result;
            }
        }

        [UsedImplicitly]
        string DumpBeforeCurrent
        {
            get
            {
                var start = Math.Max(0, Position - DumpWidth);
                var result = Source.SubString(start, Position - start);
                if(Position >= DumpWidth)
                    result = "..." + result;
                return result;
            }
        }

        [UsedImplicitly]
        string NodeDump
        {
            get
            {
                return DumpBeforeCurrent
                    + "["
                    + DumpCurrent
                    + "]"
                    + DumpAfterCurrent;
            }
        }

        public SourcePosn Clone { get { return new SourcePosn(Source, Position); } }

        public static SourcePosn operator +(SourcePosn x, int y) { return x.Source + (x.Position + y); }

        public static int operator -(SourcePosn x, SourcePosn y)
        {
            Tracer.Assert(x.Source == y.Source);
            return x.Position - y.Position;
        }

        public int? Match(IMatch automaton) { return automaton.Match(this); }

        public bool StartsWith(string data)
        {
            var length = data.Length;
            return !Source.IsEnd(Position + length - 1) && Source.SubString(Position, length) == data;
        }
    }
}