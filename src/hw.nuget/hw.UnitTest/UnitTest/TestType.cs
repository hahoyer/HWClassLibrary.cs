using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

namespace hw.UnitTest
{
    sealed class TestType : Dumpable
    {
        internal readonly Type Type;
        internal TestType(Type type) { Type = type; }
        readonly List<TestMethod> _failedMethods = new List<TestMethod>();
        bool _isSuspended;

        public IEnumerable<DependantAttribute> Dependants { get { return Type.GetAttributes<DependantAttribute>(true); } }

        IEnumerable<TestMethod> UnitTestMethods
        {
            get
            {
                return Type
                    .GetMethods()
                    .Where(methodInfo => methodInfo.GetAttribute<TestAttribute>(true) != null)
                    .Select(methodInfo => new TestMethod(methodInfo))
                    .Concat(DefaultTestMethod);
            }
        }

        IEnumerable<TestMethod> DefaultTestMethod
        {
            get
            {
                var testAttribute = Type.GetAttribute<TestFixtureAttribute>(true);
                if(testAttribute == null)
                    yield break;
                if(testAttribute.DefaultMethod == null)
                    yield break;
                yield return new TestMethod(Type.GetMethod(testAttribute.DefaultMethod));
            }
        }

        public bool IsStarted { get; set; }

        public bool IsStartable(Func<Type, bool> isLevel) { return !IsStarted && !_isSuspended && isLevel(Type); }

        public bool IsComplete { get; set; }

        public bool IsSuccessfull { get { return IsComplete && _failedMethods.Count == 0; } }

        public string ConfigurationString
        {
            get { return ConfigurationMode + " " + Type.FullName + " " + FailedMethodNames + "\n"; }
            set
            {
                var elements = value.Split(' ');
                Tracer.Assert(elements[1] == Type.FullName);
                ConfigurationMode = elements[0];
                FailedMethodNames = elements[2];
            }
        }

        string FailedMethodNames
        {
            get { return _failedMethods.Aggregate("", (current, testMethod) => current + testMethod.ConfigurationString); }
            set
            {
                var forcedMethods =
                    value.Split(',')
                        .Join(UnitTestMethods, name => name, method => method.Name, (name, method) => method)
                        .ToArray();
                foreach(var notForcedMethod in UnitTestMethods.Except(forcedMethods))
                    notForcedMethod.IsSuspended = true;
            }
        }

        string ConfigurationMode
        {
            get
            {
                if(!IsStarted || _isSuspended)
                    return "notrun";
                if(IsSuccessfull)
                    return "success";
                if(IsComplete)
                    return "error";

                return "dependanterror";
            }
            set
            {
                if(value != "error")
                    _isSuspended = true;
            }
        }

        public override string ToString() { return ConfigurationString; }

        public int ConfigurationModePriority
        {
            get
            {
                if(!IsStarted || _isSuspended)
                    return 4;
                if(IsSuccessfull)
                    return 2;
                if(IsComplete)
                    return 1;

                return 3;
            }
        }

        public void Run()
        {
            foreach(var unitTestMethod in UnitTestMethods.Where(unitTestMethod => !unitTestMethod.IsSuspended))
                try
                {
                    unitTestMethod.Run();
                }
                catch(TestFailedException)
                {
                    _failedMethods.Add(unitTestMethod);
                }
            IsComplete = true;
        }
    }
}