using System;
using System.Diagnostics;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Scanner
{
    /// <summary>
    ///     Source and position for compilation process
    /// </summary>
    [DebuggerDisplay("{" + nameof(NodeDump) + "}")]
    public sealed class SourcePosn : Dumpable, IEquatable<SourcePosn>
    {
        public static SourcePosn operator+(SourcePosn x, int y) => x.Source + (x.Position + y);

        public static int operator-(SourcePosn x, SourcePosn y)
        {
            Tracer.Assert(x.Source == y.Source);
            return x.Position - y.Position;
        }

        public static bool operator<(SourcePosn left, SourcePosn right) => left != null &&
                                                                           right != null &&
                                                                           left.Source == right.Source &&
                                                                           left.Position < right.Position;

        public static bool operator<=(SourcePosn left, SourcePosn right) => left < right || left == right;

        public static bool operator>(SourcePosn left, SourcePosn right) => right < left;
        public static bool operator>=(SourcePosn left, SourcePosn right) => right <= left;
        public static bool operator!=(SourcePosn left, SourcePosn right) => !(left == right);

        public static bool operator==(SourcePosn left, SourcePosn right)
        {
            if((object) left == null)
                return (object) right == null;
            if((object) right == null)
                return false;
            return left.Source == right.Source &&
                   left.Position == right.Position;
        }

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

        public bool Equals(SourcePosn other) => Equals(Source, other.Source) && Position == other.Position;

        public bool IsValid {get => 0 <= Position && Position <= Source.Length; set => Position = value ? 0 : -1;}

        /// <summary>
        ///     The current character
        /// </summary>
        public char Current => Source[Position];

        /// <summary>
        ///     Natuaral indexer
        /// </summary>
        public char this[int index] => Source[Position + index];

        /// <summary>
        ///     Checks if at or beyond end of source
        /// </summary>
        /// <value> </value>
        public bool IsEnd => Source.IsEnd(Position);

        [UsedImplicitly]
        string NodeDump => GetDumpAroundCurrent(Source.NodeDumpWidth);

        public SourcePosn Clone => new SourcePosn(Source, Position);

        public int LineIndex => Source.LineIndex(Position);
        public int ColumnIndex => Source.ColumnIndex(Position);
        public bool Equals(SourcePosn x, SourcePosn y) => x.Equals(y);

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj is SourcePosn posn && Equals(posn);
        }

        public override int GetHashCode() => 0;

        /// <summary>
        ///     Obtains a piece
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length) => Source.SubString(Position + start, length);

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): tag: " string</returns>
        public string FilePosn(string flagText) => Source.FilePosn(Position, Position, flagText);

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

        public string GetDumpAroundCurrent(int dumpWidth)
        {
            if(IsValid)
                return GetDumpBeforeCurrent(dumpWidth) + "[]" + GetDumpAfterCurrent(dumpWidth);
            return "<invalid>";
        }

        public int? Match(IMatch automaton) => automaton.Match(this);

        public bool StartsWith(string data, StringComparison type = StringComparison.InvariantCulture)
        {
            var length = data.Length;
            return !Source.IsEnd(Position + length - 1) && Source.SubString(Position, length).Equals(data, type);
        }

        public SourcePart Span(SourcePosn other) => SourcePart.Span(this, other);
        public SourcePart Span(int length) => SourcePart.Span(this, length);
    }
}