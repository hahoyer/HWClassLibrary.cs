using System.Collections;
using System.Reflection;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Per-<see cref="Dumper" /> configuration: an ordered chain of handlers consulted to decide how
///     a given runtime type is rendered and which of its members are visible. New defaults for common
///     framework types (collections, <see cref="Type" />, <see cref="string" />,
///     <see cref="Exception" />, primitives, enums) are pre-registered.
/// </summary>
public sealed class Configuration
{
    /// <summary>Element of the handler chain. <see cref="this" /> yields zero or more handlers that apply to a given type.</summary>
    public abstract class AbstractHandler
    {
        /// <summary>Returns the handlers from this entry that apply to <paramref name="type" />.</summary>
        public abstract IEnumerable<Handler> this[Type type] { get; }
    }

    /// <summary>Concrete pair of optional callbacks: one rendering the object, one filtering its members.</summary>
    public sealed class Handler : AbstractHandler
    {
        /// <summary>Renders an instance, or <c>null</c> to fall through to the next handler.</summary>
        public readonly Func<Type, object, string>? Dump;

        /// <summary>Decides whether a member is included in the dump, or <c>null</c> to fall through.</summary>
        public readonly Func<MemberInfo, object, bool>? MemberCheck;

        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="memberCheck"> Member visibility filter, or <c>null</c>. </param>
        public Handler(Func<Type, object, string>? dump = null, Func<MemberInfo, object, bool>? memberCheck = null)
        {
            Dump = dump;
            MemberCheck = memberCheck;
        }

        /// <inheritdoc />
        public override IEnumerable<Handler> this[Type type]
        {
            get { yield return this; }
        }
    }

    /// <summary>Wraps a handler so that it only applies when the queried type derives from or implements <see cref="Type" />.</summary>
    [PublicAPI]
    public sealed class TypedHandler : AbstractHandler
    {
        /// <summary>The wrapped handler.</summary>
        public readonly AbstractHandler Handler;

        /// <summary>Type that the queried type must satisfy via <c>is</c>-relation.</summary>
        public readonly Type Type;

        /// <param name="type"> Type that the queried type must satisfy. </param>
        /// <param name="handler"> Handler used when the type matches. </param>
        public TypedHandler(Type type, AbstractHandler handler)
        {
            Type = type;
            Handler = handler;
        }

        /// <inheritdoc />
        public override IEnumerable<Handler> this[Type type] => type.Is(Type)? Handler[type] : new Handler[0];
    }

    /// <summary>Like <see cref="TypedHandler" />, but matching is delegated to a free predicate.</summary>
    [PublicAPI]
    public sealed class MatchedTypeHandler : AbstractHandler
    {
        /// <summary>The wrapped handler.</summary>
        public readonly AbstractHandler Handler;

        /// <summary>Predicate deciding whether the queried type is a match.</summary>
        public readonly Func<Type, bool> TypeMatch;

        /// <param name="typeMatch"> Predicate identifying matching types. </param>
        /// <param name="handler"> Handler used when the predicate returns <c>true</c>. </param>
        public MatchedTypeHandler(Func<Type, bool> typeMatch, AbstractHandler handler)
        {
            TypeMatch = typeMatch;
            Handler = handler;
        }

        /// <inheritdoc />
        public override IEnumerable<Handler> this[Type type] => TypeMatch(type)? Handler[type] : new Handler[0];
    }

    /// <summary>
    ///     Ordered collection of handlers that flattens its children when queried. The order is
    ///     observed: the first matching handler with a non-<c>null</c> callback wins.
    /// </summary>
    public sealed class HandlerGroup : AbstractHandler
    {
        /// <summary>The underlying handler list, exposed for advanced manipulation.</summary>
        [PublicAPI]
        public readonly List<AbstractHandler> Handlers = new();

        /// <inheritdoc />
        public override IEnumerable<Handler> this[Type type]
            => Handlers
                .SelectMany(handler => handler[type]);

        /// <summary>Appends an arbitrary handler to the chain.</summary>
        [PublicAPI]
        public void Add(AbstractHandler handler) => Handlers.Add(handler);

        /// <summary>Appends a handler that applies whenever the queried type satisfies <paramref name="type" />.</summary>
        /// <param name="type"> Type that the queried type must satisfy via <c>is</c>-relation. </param>
        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="methodCheck"> Member visibility filter, or <c>null</c>. </param>
        [PublicAPI]
        public void Add
        (
            Type type, Func<Type, object, string>? dump = null,
            Func<MemberInfo, object, bool>? methodCheck = null
        ) => Handlers.Add(new TypedHandler(type, new Handler(dump, methodCheck)));

        /// <summary>Strongly-typed counterpart of <see cref="Add(Type, Func{Type, object, string}?, Func{MemberInfo, object, bool}?)" />.</summary>
        /// <typeparam name="TTarget"> The target type. </typeparam>
        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="methodCheck"> Member visibility filter, or <c>null</c>. </param>
        [PublicAPI]
        public void Add<TTarget>
        (
            Func<Type, TTarget, string>? dump = null,
            Func<MemberInfo, TTarget, bool>? methodCheck = null
        )
        {
            Func<Type, object, string>? actualDump = dump == null? null : (t, o) => dump(t, (TTarget)o);
            Func<MemberInfo, object, bool>? actualMethodCheck
                = methodCheck == null? null : (t, o) => methodCheck(t, (TTarget)o);
            Handlers.Add(new TypedHandler(typeof(TTarget), new Handler(actualDump, actualMethodCheck)));
        }

        /// <summary>Appends a handler whose match is decided by a free predicate.</summary>
        /// <param name="typeMatch"> Predicate identifying matching types. </param>
        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="methodCheck"> Member visibility filter, or <c>null</c>. </param>
        [PublicAPI]
        public void Add
        (
            Func<Type, bool> typeMatch, Func<Type, object, string>? dump = null,
            Func<MemberInfo, object, bool>? methodCheck = null
        ) => Handlers.Add(new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck)));

        /// <summary>Like <see cref="Add(Type, Func{Type, object, string}?, Func{MemberInfo, object, bool}?)" />, but inserts at the front so the new handler wins over earlier ones.</summary>
        /// <param name="type"> Type that the queried type must satisfy. </param>
        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="methodCheck"> Member visibility filter, or <c>null</c>. </param>
        [PublicAPI]
        public void Force
        (
            Type type, Func<Type, object, string>? dump = null,
            Func<MemberInfo, object, bool>? methodCheck = null
        ) => Handlers.Insert(0, new TypedHandler(type, new Handler(dump, methodCheck)));

        /// <summary>Predicate-based variant of <see cref="Force(Type, Func{Type, object, string}?, Func{MemberInfo, object, bool}?)" />.</summary>
        /// <param name="typeMatch"> Predicate identifying matching types. </param>
        /// <param name="dump"> Rendering callback, or <c>null</c>. </param>
        /// <param name="methodCheck"> Member visibility filter, or <c>null</c>. </param>
        [PublicAPI]
        public void Force
        (
            Func<Type, bool> typeMatch, Func<Type, object, string>? dump = null,
            Func<MemberInfo, object, bool>? methodCheck = null
        ) => Handlers.Insert(0, new MatchedTypeHandler(typeMatch, new Handler(dump, methodCheck)));
    }

    /// <summary>The active handler chain. Modify to change rendering behaviour at runtime.</summary>
    [PublicAPI]
    public readonly HandlerGroup Handlers;

    /// <summary>Creates a configuration pre-populated with handlers for common framework types.</summary>
    public Configuration()
    {
        Handlers = new();
        Handlers.Add(typeof(IList), (_, o) => Dump(((IList)o).Cast<object>()));
        Handlers.Add(typeof(IDictionary), (_, o) => DumpIDictionary(o));
        Handlers.Add(typeof(ICollection), (_, o) => Dump(((ICollection)o).Cast<object>()));
        Handlers.Add(typeof(Type), (_, o) => ((Type)o).PrettyName());
        Handlers.Add(typeof(string), (_, o) => ((string)o).CSharpQuote());
        Handlers.Add(typeof(Exception), (_, o) => ((Exception)o).Dump);
        Handlers.Add(t => t.IsEnum, DumpEnum);
        Handlers.Add(t => t.IsPrimitive, (_, o) => o.ToString() ?? "null");
        Handlers.Add(IsOutlookClass, (_, o) => o.ToString() ?? "null");
    }

    /// <summary>
    ///     Returns the rendering callback for <paramref name="type" /> from the first matching handler,
    ///     or <c>null</c> if no handler in the chain provides one.
    /// </summary>
    public Func<Type, object, string>? GetDump(Type type)
        => Handlers[type].FirstOrDefault(handler => handler.Dump != null)?.Dump;

    /// <summary>
    ///     Returns the member-visibility filter for <paramref name="type" /> from the first matching
    ///     handler, or a permissive filter (always <c>true</c>) if none is configured.
    /// </summary>
    public Func<MemberInfo, object, bool> GetMemberCheck(Type type)
    {
        var result = Handlers[type].FirstOrDefault(handler => handler.MemberCheck != null);
        return result == null? (_, o) => true : result.MemberCheck!;
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
            .ToString()!
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