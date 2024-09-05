using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public sealed class Configuration
{
    public abstract class AbstractHandler
    {
        public abstract IEnumerable<Handler> this[Type type] { get; }
    }

    public sealed class Handler : AbstractHandler
    {
        public readonly Func<Type, object, string> Dump;
        public readonly Func<MemberInfo, object, bool> MemberCheck;

        public Handler(Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> memberCheck = null)
        {
            Dump = dump;
            MemberCheck = memberCheck;
        }

        public override IEnumerable<Handler> this[Type type]
        {
            get { yield return this; }
        }
    }

    [PublicAPI]
    public sealed class TypedHandler : AbstractHandler
    {
        public readonly AbstractHandler Handler;
        public readonly Type Type;

        public TypedHandler(Type type, AbstractHandler handler)
        {
            Type = type;
            Handler = handler;
        }

        public override IEnumerable<Handler> this[Type type] => type.Is(Type)? Handler[type] : new Handler[0];
    }

    [PublicAPI]
    public sealed class MatchedTypeHandler : AbstractHandler
    {
        public readonly AbstractHandler Handler;
        public readonly Func<Type, bool> TypeMatch;

        public MatchedTypeHandler(Func<Type, bool> typeMatch, AbstractHandler handler)
        {
            TypeMatch = typeMatch;
            Handler = handler;
        }

        public override IEnumerable<Handler> this[Type type] => TypeMatch(type)? Handler[type] : new Handler[0];
    }

    public sealed class HandlerGroup : AbstractHandler
    {
        [PublicAPI]
        public readonly List<AbstractHandler> Handlers = new();

        public override IEnumerable<Handler> this[Type type]
            => Handlers
                .SelectMany(handler => handler[type])
                .Where(result => result != null);

        [PublicAPI]
        public void Add(AbstractHandler handler) => Handlers.Add(handler);

        [PublicAPI]
        public void Add
        (
            Type type, Func<Type, object, string> dump = null,
            Func<MemberInfo, object, bool> methodCheck = null
        ) => Handlers.Add(new TypedHandler(type, new Handler(dump, methodCheck)));

        [PublicAPI]
        public void Add<TTarget>
        (
            Func<Type, TTarget, string> dump = null,
            Func<MemberInfo, TTarget, bool> methodCheck = null
        )
        {
            Func<Type, object, string> actualDump = dump == null? null : (t, o) => dump(t, (TTarget)o);
            Func<MemberInfo, object, bool> actualMethodCheck
                = methodCheck == null? null : (t, o) => methodCheck(t, (TTarget)o);
            Handlers.Add(new TypedHandler(typeof(TTarget), new Handler(actualDump, actualMethodCheck)));
        }

        [PublicAPI]
        public void Add
        (
            Func<Type, bool> typeMatch, Func<Type, object, string> dump = null,
            Func<MemberInfo, object, bool> methodCheck = null
        ) => Handlers.Add(new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck)));

        [PublicAPI]
        public void Force
        (
            Type type, Func<Type, object, string> dump = null,
            Func<MemberInfo, object, bool> methodCheck = null
        ) => Handlers.Insert(0, new TypedHandler(type, new Handler(dump, methodCheck)));

        [PublicAPI]
        public void Force
        (
            Func<Type, bool> typeMatch, Func<Type, object, string> dump = null,
            Func<MemberInfo, object, bool> methodCheck = null
        ) => Handlers.Insert(0, new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck)));
    }

    public readonly HandlerGroup Handlers;

    public Configuration()
    {
        Handlers = new();
        Handlers.Add(typeof(IList), (type, o) => Dump(((IList)o).Cast<object>()));
        Handlers.Add(typeof(IDictionary), (type, o) => DumpIDictionary(o));
        Handlers.Add(typeof(ICollection), (type, o) => Dump(((ICollection)o).Cast<object>()));
        Handlers.Add(typeof(Type), (type, o) => ((Type)o).PrettyName());
        Handlers.Add(typeof(string), (type, o) => ((string)o).CSharpQuote());
        Handlers.Add(t => t.IsEnum, DumpEnum);
        Handlers.Add(t => t.IsPrimitive, (type, o) => o.ToString());
        Handlers.Add(IsOutlookClass, (type, o) => o.ToString());
    }

    public Func<Type, object, string> GetDump(Type type)
        => Handlers[type].FirstOrDefault(handler => handler.Dump != null)?.Dump;

    public Func<MemberInfo, object, bool> GetMemberCheck(Type type)
    {
        var result = Handlers[type].FirstOrDefault(handler => handler.MemberCheck != null);
        return result == null? (s, o) => true : result.MemberCheck;
    }

    static bool IsOutlookClass(Type t)
    {
        var name = t.ToString();
        return name == "Outlook.ApplicationClass"
            || name == "Outlook.NameSpaceClass"
            || name == "Outlook.InspectorClass";
    }

    static string Dump(IEnumerable<object> target)
    {
        var enumerable = target.ToArray();
        return
            "Count="
            + enumerable.Length
            + enumerable
                .Select(Tracer.Dump)
                .Stringify("\n", true)
                .Surround("{", "}");
    }

    static string DumpEnum(Type type, object target)
    {
        var result = target
            .ToString()
            .Split(',')
            .Select(item => type.PrettyName() + "." + item.Trim())
            .Stringify(", ");
        return result.Contains(",")? "(" + result + ")" : result;
    }

    static string DumpIDictionary(object target)
    {
        var dictionary = (IDictionary)target;
        return dictionary
            .Keys
            .Cast<object>()
            .Select(key => "[" + Tracer.Dump(key) + "] " + Tracer.Dump(dictionary[key]))
            .Stringify("\n")
            .Surround("{", "}");
    }
}