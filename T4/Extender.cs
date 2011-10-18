using System.IO;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;

namespace HWClassLibrary.T4
{
    public static class Extender
    {
        [UsedImplicitly]
        public static CodeGenerationContext Context(this object textTransformation) { return new CodeGenerationContext(textTransformation); }
        internal static bool IsFileContentDifferent(String fileName, string newContent) { return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent); }
    }
}