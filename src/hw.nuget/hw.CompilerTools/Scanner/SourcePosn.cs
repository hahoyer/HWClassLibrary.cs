using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Scanner
{
    /// <summary>
    ///     Source and position for compilation process
    /// </summary>
    [DebuggerDisplay("{NodeDump}")]
    public sealed class SourcePosn : Dumpable, IEquatable<SourcePosn>
    {
        public bool Equals(SourcePosn x, SourcePosn y) { return x.Equals(y); }
        public bool Equals(SourcePosn other)
        {
            return Equals(Source, other.Source) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj is SourcePosn && Equals((SourcePosn) obj);
        }

        public override int GetHashCode() { return 0; }

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

        public bool IsValid
        {
            get { return 0 <= Position && Position <= Source.Length; }
            set { Position = (value ? 0 : -1); }
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
        public string SubString(int start, int length)
        {
            return Source.SubString(Position + start, length);
        }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): tag: " string</returns>
        public string FilePosn(string flagText)
        {
            return Source.FilePosn(Position, flagText);
        }

        /// <summary>
        ///     Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion)
        {
            if(Source.IsPersistent)
                return "\n" + FilePosn("see there");
            return GetDumpAroundCurrent(Source.DumpWidth);
        }

        string GetDumpAfterCurrent(int dumpWidth)
        {
            if(IsEnd)
                return "";
            var length = Math.Min(dumpWidth, Source.Length - Position);
            var result = Source.SubString(Position, length);
            if(length == dumpWidth)
                result += "...";
            return result;
        }

        string GetDumpBeforeCurrent(int dumpWidth)
        {
            var start = Math.Max(0, Position - dumpWidth);
            var result = Source.SubString(start, Position - start);
            if(Position >= dumpWidth)
                result = "..." + result;
            return result;
        }

        [UsedImplicitly]
        string NodeDump { get { return GetDumpAroundCurrent(Source.NodeDumpWidth); } }

        public string GetDumpAroundCurrent(int dumpWidth)
        {
            if(IsValid)
                return GetDumpBeforeCurrent(dumpWidth)
                    + "[]"
                    + GetDumpAfterCurrent(dumpWidth);
            return "<invalid>";
        }

        public SourcePosn Clone { get { return new SourcePosn(Source, Position); } }
        
        public int LineIndex { get { return Source.LineIndex(Position); } }
        public int ColumnIndex { get { return Source.ColumnIndex(Position); } }

        public static SourcePosn operator +(SourcePosn x, int y)
        {
            return x.Source + (x.Position + y);
        }

        public static int operator -(SourcePosn x, SourcePosn y)
        {
            Tracer.Assert(x.Source == y.Source);
            return x.Position - y.Position;
        }

        public int? Match(IMatch automaton) { return automaton.Match(this); }

        public bool StartsWith(string data)
        {
            var length = data.Length;
            return !Source.IsEnd(Position + length - 1)
                && Source.SubString(Position, length) == data;
        }

        public SourcePart Span(SourcePosn other) { return SourcePart.Span(this, other); }
        public SourcePart Span(int length) { return SourcePart.Span(this, length); }

        public static bool operator <(SourcePosn left, SourcePosn right)
        {
            return left != null &&
                right != null &&
                left.Source == right.Source &&
                left.Position < right.Position;
        }

        public static bool operator <=(SourcePosn left, SourcePosn right)
        {
            return left < right || left == right;
        }

        public static bool operator >(SourcePosn left, SourcePosn right) { return right < left; }
        public static bool operator >=(SourcePosn left, SourcePosn right) { return right <= left; }
        public static bool operator !=(SourcePosn left, SourcePosn right)
        {
            return !(left == right);
        }

        public static bool operator ==(SourcePosn left, SourcePosn right)
        {
            if((object) left == null)
                return ((object) right == null);
            if((object) right == null)
                return false;
            return left.Source == right.Source &&
                left.Position == right.Position;
        }
    }
}