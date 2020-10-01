using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.UnitTest
{
    public sealed class SourceFilePosition
    {
        public string FileName;
        public int LineNumber;

        public string ToString(FilePositionTag tag) => Tracer.FilePosition(FileName, LineNumber, 0, tag);
    }

    sealed class TestMethod : Dumpable
    {
        interface IActor
        {
            string Name { get; }
            string LongName { get; }
            object Instance { get; }
            IEnumerable<SourceFilePosition> FilePositions { get; }
            void Run(object test);
        }

        sealed class MethodActor : IActor
        {
            readonly MethodInfo Target;
            readonly Type Type;

            public MethodActor(MethodInfo target, Type type)
            {
                Target = target;
                Type = type;
            }

            IEnumerable<SourceFilePosition> IActor.FilePositions
            {
                get
                {
                    var b = Type.GetAttribute<UnitTestAttribute>(true);
                    if(b != null)
                        yield return b.Where;
                    var a = Target.GetAttribute<UnitTestAttribute>(true);
                    if(a != null)
                        yield return a.Where;
                }
            }

            object IActor.Instance
            {
                get
                {
                    if(Target.IsStatic || Target.ReflectedType == null)
                        return null;
                    return Activator.CreateInstance(Target.ReflectedType);
                }
            }

            string IActor.LongName
            {
                get
                {
                    Tracer.Assert(Target.DeclaringType != null);
                    return Target.DeclaringType.PrettyName() + "." + Target.Name;
                }
            }

            string IActor.Name => Target.Name;
            void IActor.Run(object test) => Target.Invoke(test, new object[0]);
        }

        sealed class InterfaceActor : IActor
        {
            readonly Type Target;
            public InterfaceActor(Type target) => Target = target;

            IEnumerable<SourceFilePosition> IActor.FilePositions
            {
                get
                {
                    var b = Target.GetAttribute<UnitTestAttribute>(true);
                    if(b != null)
                        yield return b.Where;
                }
            }

            object IActor.Instance => Activator.CreateInstance(Target);
            string IActor.LongName => Target.PrettyName();
            string IActor.Name => Target.Name;
            void IActor.Run(object test) => ((ITestFixture)test).Run();
        }

        public bool IsSuspended;

        readonly IActor Actor;

        public TestMethod(MethodInfo methodInfo, Type type) => Actor = new MethodActor(methodInfo, type);

        public TestMethod(Type type) => Actor = new InterfaceActor(type);

        public string ConfigurationString => Name + ",";

        public string Name => Actor.Name;

        public void Run()
        {
            Tracer.Line("Start " + Actor.LongName);
            Tracer.IndentStart();
            Tracer.Line
            (
                Actor.FilePositions.Select
                        (p => p.ToString(FilePositionTag.Test) + " position of test")
                    .Stringify("\n"));
            try
            {
                if(!IsSuspended)
                {
                    var test = Actor.Instance;
                    var isBreakDisabled = Tracer.IsBreakDisabled;
                    Tracer.IsBreakDisabled = TestRunner.IsBreakDisabled ?? !TestRunner.IsModeErrorFocus;
                    try
                    {
                        Actor.Run(test);
                    }
                    catch(Exception e)
                    {
                        ShowException(e);
                    }

                    Tracer.IsBreakDisabled = isBreakDisabled;
                }
            }
            finally
            {
                Tracer.IndentEnd();
                Tracer.Line("End " + Actor.LongName);
            }
        }

        void ShowException(Exception e)
        {
            Tracer.Line("*********************Exception: " + Actor.LongName);
            Tracer.Line(e.GetType().FullName);
            Tracer.Line(e.Message);
            Tracer.Line("*********************End Exception: " + Actor.LongName);
            throw new TestFailedException();
        }
    }
}