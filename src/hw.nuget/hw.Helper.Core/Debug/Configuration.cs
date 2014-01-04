using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace hw.Debug
{
    public sealed class Configuration
    {
        public readonly HandlerGroup Handlers;
        internal Configuration()
        {
            Handlers = new HandlerGroup();
            Handlers.Add(typeof(IList), (type, o) => Dump(((IList) o).Cast<object>()));
            Handlers.Add(typeof(IDictionary), (type, o) => DumpIDictionary(o));
            Handlers.Add(typeof(ICollection), (type, o) => Dump(((ICollection) o).Cast<object>()));
            Handlers.Add(typeof(CodeObject), (type, o) => Dump((CodeObject) o));
            Handlers.Add(typeof(Type), (type, o) => ((Type) o).PrettyName());
            Handlers.Add(t => t.IsPrimitive || t.ToString().StartsWith("System."), (type, o) => o.ToString());
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
            if(cse != null)
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

        static string DumpIDictionary(object o)
        {
            var dictionary = (IDictionary) o;
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

        internal Func<string, object, bool> GetMemberCheck(Type type)
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
            internal readonly Func<string, object, bool> MemberCheck;
            internal readonly Func<Type, object, string> Dump;
            internal Handler(Func<Type, object, string> dump = null, Func<string, object, bool> memberCheck = null)
            {
                Dump = dump;
                MemberCheck = memberCheck;
            }
            public override IEnumerable<Handler> this[Type type] { get { yield return this; } }
        }

        public sealed class TypedHandler : AbstractHandler
        {
            readonly Type _type;
            readonly AbstractHandler _handler;
            internal TypedHandler(Type type, AbstractHandler handler)
            {
                _type = type;
                _handler = handler;
            }
            public override IEnumerable<Handler> this[Type type] { get { return type.Is(_type) ? _handler[type] : new Handler[0]; } }
        }

        public sealed class MatchedTypeHandler : AbstractHandler
        {
            readonly Func<Type, bool> _typeMatch;
            readonly AbstractHandler _handler;
            internal MatchedTypeHandler(Func<Type, bool> typeMatch, AbstractHandler handler)
            {
                _typeMatch = typeMatch;
                _handler = handler;
            }
            public override IEnumerable<Handler> this[Type type] { get { return _typeMatch(type) ? _handler[type] : new Handler[0]; } }
        }

        public sealed class HandlerGroup : AbstractHandler
        {
            readonly List<AbstractHandler> _handlers = new List<AbstractHandler>();

            public override IEnumerable<Handler> this[Type type]
            {
                get
                {
                    return _handlers
                        .SelectMany(handler => handler[type])
                        .Where(result => result != null);
                }
            }
            public void Add(AbstractHandler handler) { _handlers.Add(handler); }
            public void Add(Type type, Func<Type, object, string> dump) { _handlers.Add(new TypedHandler(type, new Handler(dump))); }
            public void Add(Func<Type, bool> typeMatch, Func<Type, object, string> dump) { _handlers.Add(new MatchedTypeHandler(typeMatch, new Handler(dump))); }
        }
    }
}