
using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

namespace hw.Scanner
{
    public sealed class Source : Dumpable
    {
        readonly string _data;
        readonly File _file;

        internal Source(File file)
        {
            _file = file;
            _data = _file.String;
        }

        public string Data { get { return _data; } }
        internal Source(string data) { _data = data; }
        internal char this[int index] { get { return IsEnd(index) ? '\0' : _data[index]; } }
        internal bool IsEnd(int posn) { return Length <= posn; }
        internal int Length { get { return _data.Length; } }
        internal string SubString(int start, int length) { return _data.Substring(start, length); }

        internal string FilePosn(int i, string flagText, string tag = null)
        {
            if(_file == null)
                return "????";
            return Tracer.FilePosn(_file.FullName, LineNr(i), ColNr(i) + 1, tag ?? FilePositionTag.Debug.ToString()) + flagText;
        }

        int LineNr(int iEnd) { return _data.Take(iEnd).Count(c => c == '\n'); }

        int ColNr(int iEnd) { return _data.Take(iEnd).Aggregate(0, (current, c) => c == '\n' ? 0 : current + 1); }

        protected override string Dump(bool isRecursion) { return FilePosn(0, "see there"); }

        public static SourcePosn operator +(Source x, int y) { return new SourcePosn(x, y); }
    }
}