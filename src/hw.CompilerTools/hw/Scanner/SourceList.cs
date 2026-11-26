// ReSharper disable CheckNamespace
using System.Collections;
using hw.DebugFormatter;

namespace hw.Scanner;

sealed class SourceList
    : DumpableObject
        , ISourceProvider
        , IMultiSourceProvider
        , ITextProvider
{
    sealed class Enumerator : IEnumerator<char>
    {
        readonly SourceList Parent;
        int CurrentIndex;
        IEnumerator<char> CurrentFile;

        char Current => CurrentFile.Current;

        public Enumerator(SourceList parent)
        {
            Parent = parent;
            CurrentFile = Parent.Target[CurrentIndex].Data.GetEnumerator();
        }

        public void Dispose() => CurrentFile.Dispose();

        object IEnumerator.Current => Current;

        bool IEnumerator.MoveNext()
        {
            if(CurrentFile.MoveNext())
                return true;
            CurrentFile.Dispose();
            CurrentIndex++;
            if(CurrentIndex >= Parent.Target.Length)
                return false;
            CurrentFile = Parent.Target[CurrentIndex].Data.GetEnumerator();
            return true;
        }

        void IEnumerator.Reset()
        {
            CurrentFile.Dispose();
            CurrentIndex = 0;
            CurrentFile = Parent.Target[CurrentIndex].Data.GetEnumerator();
        }

        char IEnumerator<char>.Current => Current;
    }

    readonly Source[] Target;

    int Length => Target.Sum(t => t.Length);

    string this[Range range]
    {
        get
        {
            var start = range.Start.GetOffset(Length);
            var end = range.End.GetOffset(Length);
            var result = "";
            foreach(var source in Target)
            {
                var partStart = Restrict(start, source.Length);
                var partEnd = Restrict(end, source.Length);
                result += source[partStart..partEnd];
                start -= source.Length;
                end -= source.Length;
            }

            return result;
        }
    }

    char this[Index index]
    {
        get
        {
            var position = index.GetOffset(Length);
            foreach(var source in Target)
            {
                if(position < source.Length)
                    return source[new Index(position)];
                position -= source.Length;
            }

            return '\0';
        }
    }

    public SourceList(IEnumerable<ISourceProvider> target)
        => Target = target
            .Select(t => new Source(t, t.Identifier))
            .ToArray();

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => new Enumerator(this);

    SourcePosition IMultiSourceProvider.Position(int position, bool isEnd)
        => Position(position, isEnd);


    ITextProvider ISourceProvider.Data => this;
    string? ISourceProvider.Identifier => null;
    bool ISourceProvider.IsPersistent => Target.Any(t => t.IsPersistent);
    int ISourceProvider.Length => Length;

    string ITextProvider.this[Range range] => this[range];

    char ITextProvider.this[Index index] => this[index];

    SourcePosition Position(int position, bool isEnd)
    {
        foreach(var source in Target)
        {
            var end = source.Length;
            if(position < end || (position == end && isEnd))
            {
                if(source.SourceProvider is IMultiSourceProvider provider)
                    return provider.Position(position, isEnd);
                return source + position;
            }

            position -= end;
        }

        throw new ArgumentOutOfRangeException(nameof(position));
    }

    static int Restrict(int start, int length) => T(T(start, 0).Max(), length).Min();
}
