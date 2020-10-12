using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.UnitTest
{
    sealed class TestType : DumpableObject, ValueCache.IContainer
    {
        public bool IsStarted { get; set; }
        internal readonly List<TestMethod> FailedMethods = new List<TestMethod>();
        internal bool IsComplete { get; set; }
        internal readonly Type Type;
        bool IsSuspended;
        internal TestType(Type type) => Type = type;

        public string ConfigurationString
        {
            get => ConfigurationMode + " " + Type.FullName + " " + FailedMethodNames + "\n";
            set
            {
                var elements = value.Split(' ');
                Tracer.Assert(elements[1] == Type.FullName);
                ConfigurationMode = elements[0];
                FailedMethodNames = elements[2];
            }
        }

        internal IEnumerable<DependenceProvider> DependenceProviders => Type.GetAttributes<DependenceProvider>(true);


        [PublicAPI]
        [Obsolete("Use IsSuccessful")]
        // ReSharper disable once IdentifierTypo
        internal bool IsSuccessfull => IsSuccessful;

        internal bool IsSuccessful => IsComplete && FailedMethods.Count == 0;

        TestMethod[] UnitTestMethodsCache;

        internal TestMethod[] UnitTestMethods => UnitTestMethodsCache ??= GetUnitTestMethods();
        
        TestMethod[] GetUnitTestMethods()
        => Type
                .GetMethods()
                .Where(IsUnitTestMethod)
                .Select(methodInfo => new TestMethod(methodInfo))
                .Concat(DefaultTestMethods)
                .Concat(InterfaceMethods)
                .ToArray();

        IEnumerable<TestMethod> InterfaceMethods
        {
            get
            {
                if(Type.Is<ITestFixture>())
                    yield return new TestMethod(Type);
            }
        }

        IEnumerable<TestMethod> DefaultTestMethods
        {
            get
            {
                var testAttribute = Type.GetAttribute<UnitTestAttribute>(true);
                if(testAttribute?.DefaultMethod != null)
                    yield return new TestMethod(Type.GetMethod(testAttribute.DefaultMethod));
            }
        }

        string FailedMethodNames
        {
            get => FailedMethods.Aggregate("", (current, testMethod) => current + testMethod.ConfigurationString);
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
            // ReSharper disable once StringLiteralTypo
                => !IsStarted || IsSuspended? "notrun" :
                    IsSuccessful? "success" :
                    IsComplete? "error" :
                    UnitTestMethods.Any(m=>m.IsActive)? "active":
                    // ReSharper disable once StringLiteralTypo
                    "dependanterror";
            set
            {
                if(value == "success" )
                    IsSuspended = true;
            }
        }

        internal int ConfigurationModePriority
            => !IsStarted || IsSuspended? 4 :
                IsSuccessful? 2 :
                IsComplete? 1 :
                3;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [PublicAPI]
        [Obsolete("Use CanBeStarted")]
        // ReSharper disable once IdentifierTypo
        public bool IsStartable(Func<Type, bool> isLevel) => CanBeStarted(isLevel);

        public bool CanBeStarted(Func<Type, bool> isLevel) => !IsStarted && !IsSuspended && isLevel(Type);

        public override string ToString() => ConfigurationString;

        static bool IsUnitTestMethod(MethodInfo methodInfo)
            => methodInfo.GetAttribute<UnitTestAttribute>(true) != null
               || TestRunner.RegisteredFrameworks.Any(item => item.IsUnitTestMethod(methodInfo));

        public int GetPriority(TestMethod method)
            => method.IsActive? 0 :
                !IsStarted || IsSuspended? 4 :
                IsSuccessful? 2 :
                IsComplete? 1 :
                3;

        protected override string GetNodeDump() => Type.PrettyName();

        public string GetMode(TestMethod method) => method.IsActive? "active" : ConfigurationMode;
    }
}