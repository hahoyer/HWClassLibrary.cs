using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;

namespace hw.DebugFormatter
{
    public sealed class Configuration
    {
        public readonly HandlerGroup Handlers;
        internal Configuration()
        {
            Handlers = new HandlerGroup();
            Handlers.Add(typeof(IList), (type, o) => Dump(((IList)o).Cast<object>()));
            Handlers.Add(typeof(IDictionary), (type, o) => DumpIDictionary(o));
            Handlers.Add(typeof(ICollection), (type, o) => Dump(((ICollection)o).Cast<object>()));
            Handlers.Add(typeof(CodeObject), (type, o) => Dump((CodeObject)o));
            Handlers.Add(typeof(Type), (type, o) => ((Type)o).PrettyName());
            Handlers.Add(typeof(string), (type, o) => ((string)o).Quote());
            Handlers.Add(t => t.IsEnum, DumpEnum);
            Handlers.Add(t => t.IsPrimitive, (type, o) => o.ToString());
            Handlers.Add(IsOutlookClass, (type, o) => o.ToString());
        }
        static bool IsOutlookClass(Type t)
        {
            var name = t.ToString();
            return name == "Outlook.ApplicationClass"
                || name == "Outlook.NameSpaceClass"
                || name == "Outlook.InspectorClass";
        }


        static string Dump(CodeObject co)
        {
            var cse = co as CodeSnippetExpression;
            if (cse != null)
                return cse.Value;

            throw new NotImplementedException();
        }

        static string Dump(IEnumerable<object> objects)
        {
            var enumerable = objects.ToArray();
            return
                "Count=" + enumerable.Length +
                    enumerable
                        .Select(Tracer.Dump)
                        .Stringify("\n", true)
                        .Surround("{", "}");
        }

        static string DumpEnum(Type type, object o)
        {
            var result = o
                .ToString()
                .Split(',')
                .Select(item => type.PrettyName() + "." + item)
                .Stringify(", ");
            if (result.Contains(","))
                return "(" + result + ")";
            return result;
        }

        static string DumpIDictionary(object o)
        {
            var dictionary = (IDictionary)o;
            return dictionary
                .Keys
                .Cast<object>()
                .Select(key => "[" + Tracer.Dump(key) + "] " + Tracer.Dump(dictionary[key]))
                .Stringify("\n")
                .Surround("{", "}");
        }

        internal Func<Type, object, string> GetDump(Type type)
        {
            var result = Handlers[type].FirstOrDefault(handler => handler.Dump != null);
            return result == null ? null : result.Dump;
        }

        internal Func<MemberInfo, object, bool> GetMemberCheck(Type type)
        {
            var result = Handlers[type].FirstOrDefault(handler => handler.MemberCheck != null);
            return result == null ? ((s, o) => true) : result.MemberCheck;
        }

        public abstract class AbstractHandler
        {
            public abstract IEnumerable<Handler> this[Type type] { get; }
        }

        public sealed class Handler : AbstractHandler
        {
            public readonly Func<MemberInfo, object, bool> MemberCheck;
            public readonly Func<Type, object, string> Dump;
            public Handler(Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> memberCheck = null)
            {
                Dump = dump;
                MemberCheck = memberCheck;
            }
            public override IEnumerable<Handler> this[Type type] { get { yield return this; } }
        }

        public sealed class TypedHandler : AbstractHandler
        {
            public readonly Type Type;
            public readonly AbstractHandler Handler;
            public TypedHandler(Type type, AbstractHandler handler)
            {
                Type = type;
                Handler = handler;
            }
            public override IEnumerable<Handler> this[Type type] { get { return type.Is(Type) ? Handler[type] : new Handler[0]; } }
        }

        public sealed class MatchedTypeHandler : AbstractHandler
        {
            public readonly Func<Type, bool> TypeMatch;
            public readonly AbstractHandler Handler;
            public MatchedTypeHandler(Func<Type, bool> typeMatch, AbstractHandler handler)
            {
                TypeMatch = typeMatch;
                Handler = handler;
            }
            public override IEnumerable<Handler> this[Type type] { get { return TypeMatch(type) ? Handler[type] : new Handler[0]; } }
        }

        public sealed class HandlerGroup : AbstractHandler
        {
            public readonly List<AbstractHandler> Handlers = new List<AbstractHandler>();

            public override IEnumerable<Handler> this[Type type]
            {
                get
                {
                    return Handlers
                        .SelectMany(handler => handler[type])
                        .Where(result => result != null);
                }
            }

            public void Add(AbstractHandler handler) { Handlers.Add(handler); }
            public void Add(Type type, Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> methodCheck = null) { Handlers.Add(new TypedHandler(type, new Handler(dump, methodCheck))); }
            public void Add(Func<Type, bool> typeMatch, Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> methodCheck = null) { Handlers.Add(new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck))); }
            public void Force(Type type, Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> methodCheck = null) { Handlers.Insert(0, new TypedHandler(type, new Handler(dump, methodCheck))); }
            public void Force(Func<Type, bool> typeMatch, Func<Type, object, string> dump = null, Func<MemberInfo, object, bool> methodCheck = null) { Handlers.Insert(0, new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck))); }
        }
    }
}