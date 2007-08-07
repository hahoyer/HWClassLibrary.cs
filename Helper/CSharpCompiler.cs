

using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using HWClassLibrary.IO;
using Microsoft.CSharp;
using NUnit.Framework;

namespace HWClassLibrary.Helper
{
    static class CSharpCompiler
    {
        public static object Exec(string fileName, string namespaceName, string typeName, string methodName, params object[] args)
        {
            // Build the parameters for source compilation.
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateInMemory = true;
            cp.IncludeDebugInformation = true;
            CompilerResults cr = new CSharpCodeProvider().CompileAssemblyFromFile(cp, fileName);
            if (cr.Errors.Count > 0)
                throw new CSharpCompilerErrors(cr.Errors);
            cp.ReferencedAssemblies.Add("HWClassLibrary.dll");
            MethodInfo methodInfo = FindMethod(cr.CompiledAssembly, namespaceName, typeName, methodName);
            return methodInfo.Invoke(null, args);
        }

        private static MethodInfo FindMethod(_Assembly assembly, string namespaceName, string typeName, string methodName)
        {
            Type type = FindType(assembly, namespaceName, typeName);
            return FindMethod(type, methodName);
        }

        private static Type FindType(_Assembly assembly, string namespaceName, string typeName)
        {
            if (namespaceName != "?" && typeName != "?")
            {
                string typeFullName = typeName;
                if (namespaceName != "")
                    typeFullName = namespaceName + "." + typeName;
                return assembly.GetType(typeFullName);
            }
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (IsMatch(types[i], namespaceName, typeName))
                    return types[i];
            }
            return null;
        }

        private static bool IsMatch(Type type, string namespaceName, string typeName)
        {
            if (namespaceName != "?")
                return type.Namespace == namespaceName;
            return type.Name == typeName;
        }

        private static MethodInfo FindMethod(Type type, string methodName)
        {
            if (methodName == "?")
                return type.GetMethods()[0];
            return type.GetMethod(methodName);
        }
    }

    class CSharpCompilerErrors : Exception
    {
        public CompilerErrorCollection CompilerErrorCollection { get { return _compilerErrorCollection; } }
        private readonly CompilerErrorCollection _compilerErrorCollection;

        public CSharpCompilerErrors(CompilerErrorCollection compilerErrorCollection)
        {
            _compilerErrorCollection = compilerErrorCollection;
        }
    }

    [TestFixture]
    public class Test
    {
        /// <summary>
        /// Special test, will not work automatically.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test,Explicit]
        public void TestMethod()
        {
            object x1 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            object x2 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            object x3 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            object x4 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
            object x5 = CSharpCompiler.Exec(File.SourceFileName(0), "?", "?", "?", null);
        }
    }
}
