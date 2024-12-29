using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public class Compiler<TSourcePart> : DumpableObject
    where TSourcePart : class
{
    public interface IComponent
    {
        Component Current { set; }
    }

    [PublicAPI]
    public sealed class Component
    {
        public readonly Compiler<TSourcePart> Parent;
        public readonly object Tag;

        public PrioTable PrioTable
        {
            set => Parent.Define(value, null, null, Tag);
        }

        public ITokenFactory<TSourcePart> TokenFactory
        {
            set => Parent.Define(null, value, null, Tag);
        }

        public Func<TSourcePart, IParserTokenType<TSourcePart>> BoxFunction
        {
            set => Parent.Define(null, null, value, Tag);
        }

        public IParser<TSourcePart>? Parser => Parent.Dictionary[Tag].Parser;
        public ISubParser<TSourcePart> SubParser => Parent.Dictionary[Tag].SubParser;

        internal Component(Compiler<TSourcePart> parent, object tag)
        {
            Parent = parent;
            Tag = tag;
        }

        public T? Get<T>() => Parent.Dictionary[Tag].Get<T>();
        public void Add<T>(T value) => Parent.Dictionary[Tag].Add(value, this);
    }

    sealed class ComponentData : DumpableObject, ValueCache.IContainer
    {
        static ComponentData() => Tracer.Dumper.Configuration.Handlers.Add(typeof(Delegate), (_, _) => "?");

        readonly IDictionary<Type, object> Components =
            new Dictionary<Type, object>();

        public string PrettyDump
            => Components
                .Select(p => PrettyDumpPair(p.Key, p.Value))
                .Stringify("\n");

        internal IParser<TSourcePart>? Parser => this.CachedValue(CreateParser);
        internal ISubParser<TSourcePart> SubParser => this.CachedValue(CreateSubParser);

        Func<TSourcePart, IParserTokenType<TSourcePart>>? Converter =>
            Get<Func<TSourcePart, IParserTokenType<TSourcePart>>>();

        PrioTable? PrioTable => Get<PrioTable>();
        ITokenFactory<TSourcePart>? TokenFactory => Get<ITokenFactory<TSourcePart>>();

        internal ComponentData
        (
            PrioTable? prioTable,
            ITokenFactory<TSourcePart>? tokenFactory,
            Func<TSourcePart, IParserTokenType<TSourcePart>>? converter,
            Component component
        )
        {
            Add(prioTable, component);
            Add(tokenFactory, component);
            Add(converter, component);
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        internal ComponentData ReCreate
        (
            PrioTable? prioTable,
            ITokenFactory<TSourcePart>? tokenFactory,
            Func<TSourcePart, IParserTokenType<TSourcePart>>? converter,
            Component t
        )
            =>
                new(prioTable ?? PrioTable, tokenFactory ?? TokenFactory, converter ?? Converter, t);

        internal T? Get<T>() => (T?)Components[typeof(T)];

        internal void Add<T>(T? value, Component parent)
        {
            if(ReferenceEquals(value, default(T)))
                return;
            Components.Add(typeof(T), value!);

            if(value is IComponent component)
                component.Current = parent;
        }

        static string PrettyDumpPair(Type key, object value)
            => key.PrettyName() + "=" + ("\n" + PrettyDumpValue(value)).Indent();

        static string PrettyDumpValue(object value) => Tracer.Dump(value);

        ISubParser<TSourcePart> CreateSubParser() => new SubParser<TSourcePart>(Parser!, Converter!);

        IParser<TSourcePart>? CreateParser()
        {
            if(PrioTable == null)
                return null;

            ITokenFactory<TSourcePart> tokenFactory = new CachingTokenFactory<TSourcePart>(TokenFactory!);
            var beginOfText = tokenFactory.BeginOfText;
            if(beginOfText == null)
                return null;

            return new PrioParser<TSourcePart>
            (
                PrioTable,
                new TwoLayerScanner(tokenFactory),
                beginOfText
            );
        }
    }

    readonly IDictionary<object, ComponentData> Dictionary =
        new Dictionary<object, ComponentData>();

    public Component this[object tag] => new(this, tag);

    public string PrettyDump
        => Dictionary
            .Select(p => PrettyDumpPair(p.Key, p.Value))
            .Stringify("\n");

    void Define
    (
        PrioTable? prioTable,
        ITokenFactory<TSourcePart>? tokenFactory,
        Func<TSourcePart, IParserTokenType<TSourcePart>>? converter,
        object tag
    )
        => Dictionary[tag] =
            Dictionary.TryGetValue(tag, out var componentData)
                ? componentData.ReCreate(prioTable, tokenFactory, converter, this[tag])
                : new(prioTable, tokenFactory, converter, this[tag]);

    static string PrettyDumpPair(object key, ComponentData value) => key + "=" + ("\n" + value.PrettyDump).Indent();
}