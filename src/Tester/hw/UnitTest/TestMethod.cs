using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.UnitTest;

[PublicAPI]
sealed class TestMethod : DumpableObject
{
    internal abstract class ActorBase : Dumpable
    {
        internal readonly MethodInfo MethodInfo;
        internal Type InstanceType;

        protected ActorBase(MethodInfo methodInfo, Type instanceType = null)
        {
            InstanceType = instanceType ?? methodInfo.DeclaringType;
            (InstanceType != null).Assert();
            MethodInfo = methodInfo;
        }

        internal virtual object Instance => IsStatic? null : Activator.CreateInstance(InstanceType);
        internal virtual void Run(object target) => MethodInfo.Invoke(target, new object[0]);
        bool IsStatic => MethodInfo.IsStatic;
    }

    sealed class MethodActor : ActorBase
    {
        internal MethodActor(MethodInfo methodInfo)
            : base(methodInfo) { }
    }

    sealed class ActionActor : ActorBase
    {
        internal override object Instance { get; }

        internal ActionActor(Action action)
            : base(action.Method, action.Target.GetType())
            => Instance = action.Target;
    }

    sealed class InterfaceActor : ActorBase
    {
        internal InterfaceActor(Type target)
            : base(typeof(ITestFixture).GetMethod("Run"))
            => InstanceType = target;

        internal override void Run(object target) => ((ITestFixture)target).Run();
    }

    public bool IsActive;
    public bool IsSuspended;
    public bool? IsSuccessful;

    internal readonly ActorBase Actor;

    public TestMethod(MethodInfo methodInfo) => Actor = new MethodActor(methodInfo);

    public TestMethod(Type type) => Actor = new InterfaceActor(type);
    public TestMethod(Action action) => Actor = new ActionActor(action);

    protected override string GetNodeDump() => LongName;

    public string LongName => InstanceTypeName + "." + Name;

    string InstanceTypeName => Actor.InstanceType.CompleteName();
    public string ConfigurationString => Name + ",";
    public string Name => Actor.MethodInfo.Name;
    public string RunString => $"TestRunner.RunTest(new {InstanceTypeName}().{Name})";

    internal IEnumerable<SourceFilePosition> FilePositions
    {
        get
        {
            var instance = Actor.InstanceType.GetAttributes<ILocationProvider>(true).FirstOrDefault();
            if(instance != null)
                yield return instance.Where;
            var method = Actor.MethodInfo?.GetAttributes<ILocationProvider>(true).FirstOrDefault();
            if(method != null)
                yield return method.Where;
        }
    }

    public void Run()
    {
        ("Start " + LongName).Log();
        Tracer.IndentStart();
        FilePositions.Select
                (p => p.ToString(FilePositionTag.Test) + " position of test")
            .Stringify("\n").Log();
        try
        {
            if(!IsSuspended)
            {
                var test = Actor.Instance;
                var isBreakDisabled = Tracer.IsBreakDisabled;
                try
                {
                    Tracer.IsBreakDisabled = !TestRunner.Configuration.IsBreakEnabled;
                    Actor.Run(test);
                }
                catch(Exception e)
                {
                    ShowException(e);
                }
                finally
                {
                    Tracer.IsBreakDisabled = isBreakDisabled;
                }
            }
        }
        finally
        {
            Tracer.IndentEnd();
            ("End " + LongName).Log();
        }
    }

    void ShowException(Exception e)
    {
        ("*********************Exception: " + LongName).Log();
        e.GetType().FullName.Log();
        e.Message.Log();
        ("*********************End Exception: " + LongName).Log();
        throw new TestFailedException();
    }
}