using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;

namespace hw.UnitTest
{
    public sealed class SourceFilePosition
    {
        public string FileName;
        public int LineNumber;
        public string ToString(FilePositionTag tag)
        {
            return Tracer.FilePosn(FileName, LineNumber, 0, tag);
        }
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
            readonly Type _type;
            readonly MethodInfo _target;
            public MethodActor(MethodInfo target, Type type)
            {
                _target = target;
                _type = type;
            }
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
            IEnumerable<SourceFilePosition> IActor.FilePositions
            {
                get
                {
                    var b = _type.GetAttribute<UnitTestAttribute>(true);
                    if(b != null)
                        yield return b.Where;
                    var a = _target.GetAttribute<UnitTestAttribute>(true);
                    if(a != null)
                        yield return a.Where;
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
            IEnumerable<SourceFilePosition> IActor.FilePositions
            {
                get
                {
                    var b = _target.GetAttribute<UnitTestAttribute>(true);
                    if(b != null)
                        yield return b.Where;
                }
            }
            void IActor.Run(object test) { ((ITestFixture) test).Run(); }
        }

        readonly IActor _actor;
        public bool IsSuspended;
        public TestMethod(MethodInfo methodInfo, Type type)
        {
            _actor = new MethodActor(methodInfo, type);
        }

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
            Tracer.Line
                (
                    _actor.FilePositions.Select
                        (p => p.ToString(FilePositionTag.Test) + " position of test")
                        .Stringify("\n"));
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