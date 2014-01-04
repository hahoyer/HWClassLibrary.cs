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
        readonly HandlerGroup _handlers;
        internal Configuration()
        {
            _handlers = new HandlerGroup();
            _handlers.Add(typeof(IList), (type, o) => Dump(((IList) o).Cast<object>()));
            _handlers.Add(typeof(IDictionary), (type, o) => DumpIDictionary(o));
            _handlers.Add(typeof(ICollection), (type, o) => Dump(((ICollection) o).Cast<object>()));
            _handlers.Add(typeof(CodeObject), (type, o) => Dump((CodeObject) o));
            _handlers.Add(typeof(Type), (type, o) => ((Type) o).PrettyName());
            _handlers.Add(t => t.IsPrimitive || t.ToString().StartsWith("System."), (type, o) => o.ToString());
            _handlers.Add(IsOutlookClass, (type, o) => o.ToString());
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

        internal Handler GetHandler(Type type) { return _handlers[type]; }

        public abstract class AbstractHandler
        {
            public abstract Handler this[Type type] { get; }
        }

        public sealed class Handler : AbstractHandler
        {
            internal readonly Func<Type, object, string> Dump;
            internal Handler(Func<Type, object, string> dump) { Dump = dump; }
            public override Handler this[Type type] { get { return this; } }
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
            public override Handler this[Type type] { get { return type.Is(_type) ? _handler[type] : null; } }
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
            public override Handler this[Type type] { get { return _typeMatch(type) ? _handler[type] : null; } }
        }

        public sealed class HandlerGroup : AbstractHandler
        {
            readonly List<AbstractHandler> _handlers = new List<AbstractHandler>();

            public override Handler this[Type type]
            {
                get
                {
                    return _handlers
                        .Select(handler => handler[type])
                        .FirstOrDefault(result => result != null);
                }
            }
            public void Add(AbstractHandler handler) { _handlers.Add(handler); }
            public void Add(Type type, Func<Type, object, string> dump) { _handlers.Add(new TypedHandler(type, new Handler(dump))); }
            public void Add(Func<Type, bool> typeMatch, Func<Type, object, string> dump) { _handlers.Add(new MatchedTypeHandler(typeMatch, new Handler(dump))); }
        }
    }
}