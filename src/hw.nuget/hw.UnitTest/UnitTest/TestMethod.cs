using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Debug;
using hw.Helper;

namespace hw.UnitTest
{
    public class SourceFilePosition
    {
        public string FileName;
        public int LineNumber;
        public string ToString(FilePositionTag tag) { return Tracer.FilePosn(FileName, LineNumber, 0, tag); }
    }

    sealed class TestMethod : Dumpable
    {
        interface IActor
        {
            string Name { get; }
            string LongName { get; }
            object Instance { get; }
            SourceFilePosition FilePosition { get; }
            void Run(object test);
        }

        sealed class MethodActor : IActor
        {
            readonly MethodInfo _target;
            public MethodActor(MethodInfo target) { _target = target; }
            string IActor.Name { get { return _target.Name; } }
            string IActor.LongName
            {
                get
                {
                    Tracer.Assert(_target.DeclaringType != null);
                    return _target.DeclaringType.PrettyName() + "." + _target.Name;
                }
            }
            object IActor.Instance
            {
                get
                {
                    if(_target.IsStatic || _target.ReflectedType == null)
                        return null;
                    return Activator.CreateInstance(_target.ReflectedType);
                }
            }
            SourceFilePosition IActor.FilePosition
            {
                get
                {
                    var a = _target.GetAttribute<TestAttribute>(true);
                    if(a != null)
                        return a.Where;
                    return _target.DeclaringType.GetAttribute<TestFixtureAttribute>(true)
                        .AssertNotNull()
                        .Where;
                }
            }
            void IActor.Run(object test) { _target.Invoke(test, new object[0]); }
        }

        sealed class InterfaceActor : IActor
        {
            readonly Type _target;
            public InterfaceActor(Type target) { _target = target; }
            string IActor.Name { get { return _target.Name; } }
            string IActor.LongName { get { return _target.PrettyName(); } }
            object IActor.Instance { get { return Activator.CreateInstance(_target); } }
            SourceFilePosition IActor.FilePosition
            {
                get
                {
                    return _target.GetAttribute<TestFixtureAttribute>(true)
                        .AssertNotNull()
                        .Where;
                }
            }
            void IActor.Run(object test) { ((ITestFixture) test).Run(); }
        }

        readonly IActor _actor;
        public bool IsSuspended;
        public TestMethod(MethodInfo methodInfo) { _actor = new MethodActor(methodInfo); }

        public TestMethod(Type type) { _actor = new InterfaceActor(type); }

        public string ConfigurationString { get { return Name + ","; } }

        public string Name { get { return _actor.Name; } }

        void ShowException(Exception e)
        {
            Tracer.Line("*********************Exception: " + _actor.LongName);
            Tracer.Line(e.GetType().FullName);
            Tracer.Line(e.Message);
            Tracer.Line("*********************End Exception: " + _actor.LongName);
            throw new TestFailedException();
        }

        public void Run()
        {
            Tracer.Line("Start " + _actor.LongName);
            Tracer.IndentStart();
            Tracer.Line(_actor.FilePosition.ToString(FilePositionTag.Test)+ " position of test");
            try
            {
                if(!IsSuspended)
                {
                    var test = _actor.Instance;
                    var isBreakDisabled = Tracer.IsBreakDisabled;
                    Tracer.IsBreakDisabled = !TestRunner.IsModeErrorFocus;
                    try
                    {
                        _actor.Run(test);
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
                Tracer.Line("End " + _actor.LongName);
            }
        }
    }
}