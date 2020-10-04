using System;
using System.Runtime.CompilerServices;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Helper
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [MeansImplicitUse]
    [PublicAPI]
    public class LocationProviderAttribute : Attribute, ILocationProvider
    {
        public SourceFilePosition Where;

        public LocationProviderAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
            => Where = new SourceFilePosition
            {
                FileName = fileName, LineNumber = lineNumber
            };

        SourceFilePosition ILocationProvider.Where => Where;
    }

    [PublicAPI]
    public sealed class SourceFilePosition
    {
        public string FileName;
        public int LineNumber;


        public string ToString(FilePositionTag tag)
            => Tracer.FilePosition(FileName, new TextPart {Start = new TextPosition {LineNumber = LineNumber}}, tag);
    }

    [PublicAPI]
    public interface ILocationProvider 
    {
        SourceFilePosition Where { get; }
    }
}