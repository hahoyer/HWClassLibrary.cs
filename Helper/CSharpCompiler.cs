using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using Microsoft.CSharp;
using NUnit.Framework;

namespace HWClassLibrary.Helper
{
    internal static class CSharpCompiler
    {
        public static object Exec(string fileName, string namespaceName, string typeName, string methodName, params object[] args)
        {
            // Build the parameters for source compilation.
            var cp = new CompilerParameters { GenerateInMemory = true, IncludeDebugInformation = true, CompilerOptions = "/TargetFrameworkVersion=v3.5" };
            cp.ReferencedAssemblies.AddRange(new []{"System.dll", "NUnit.Framework.dll", "HWClassLibrary.dll"});
            var cr = new CSharpCodeProvider().CompileAssemblyFromFile(cp, fileName);
            if(cr.Errors.Count > 0)

            {
                foreach (var error in cr.Errors)
                {
                    Tracer.Line(error.ToString());
                }
                
                throw new CSharpCompilerErrors(cr.Errors);
            }
            var methodInfo = FindMethod(cr.CompiledAssembly, namespaceName, typeName, methodName);
            return methodInfo.Invoke(null, args);
        }

        private static MethodInfo FindMethod(_Assembly assembly, string namespaceName, string typeName, string methodName)
        {
            var type = FindType(assembly, namespaceName, typeName);
            return FindMethod(type, methodName);
        }

        private static Type FindType(_Assembly assembly, string namespaceName, string typeName)
        {
            if(namespaceName != "?" && typeName != "?")
            {
                var typeFullName = typeName;
                if(namespaceName != "")
                    typeFullName = namespaceName + "." + typeName;
                return assembly.GetType(typeFullName);
            }
            var types = assembly.GetTypes();
            for(var i = 0; i < types.Length; i++)
                if(IsMatch(types[i], namespaceName, typeName))
                    return types[i];
            return null;
        }

        private static bool IsMatch(Type type, string namespaceName, string typeName)
        {
            if(namespaceName != "?")
                return type.Namespace == namespaceName;
            return type.Name == typeName;
        }

        private static MethodInfo FindMethod(Type type, string methodName)
        {
            if(methodName == "?")
                return type.GetMethods()[0];
            return type.GetMethod(methodName);
        }
    }

    internal class CSharpCompilerErrors : Exception
    {
        public CompilerErrorCollection CompilerErrorCollection { get { return _compilerErrorCollection; } }
        private readonly CompilerErrorCollection _compilerErrorCollection;

        public CSharpCompilerErrors(CompilerErrorCollection compilerErrorCollection) { _compilerErrorCollection = compilerErrorCollection; }
    }

    [TestFixture]
    public class Test
    {
        /// <summary>
        /// Special test, will not work automatically.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test, Explicit]
        public void TestMethod()
        {
            var x1 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            var x2 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            var x3 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            var x4 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            var x5 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
        }
    }
}