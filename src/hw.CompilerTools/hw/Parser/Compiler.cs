using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public class Compiler<TParserResult> : DumpableObject
    where TParserResult : class
{
    public interface IComponent
    {
        Component Current { set; }
    }

    [PublicAPI]
    public sealed class Component
    {
        public readonly Compiler<TParserResult> Parent;
        public readonly object Tag;

        public PrioTable PrioTable
        {
            set => Parent.Define(value, null, null, Tag);
        }

        public ITokenFactory<TParserResult> TokenFactory
        {
            set => Parent.Define(null, value, null, Tag);
        }

        public Func<TParserResult, IParserTokenType<TParserResult>> BoxFunction
        {
            set => Parent.Define(null, null, value, Tag);
        }

        public IParser<TParserResult> Parser => Parent.Dictionary[Tag].Parser!;
        public ISubParser<TParserResult> SubParser => Parent.Dictionary[Tag].SubParser;

        internal Component(Compiler<TParserResult> parent, object tag)
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

        internal IParser<TParserResult>? Parser => this.CachedValue(CreateParser);
        internal ISubParser<TParserResult> SubParser => this.CachedValue(CreateSubParser);

        Func<TParserResult, IParserTokenType<TParserResult>>? Converter =>
            Get<Func<TParserResult, IParserTokenType<TParserResult>>>();

        PrioTable? PrioTable => Get<PrioTable>();
        ITokenFactory<TParserResult>? TokenFactory => Get<ITokenFactory<TParserResult>>();

        internal ComponentData
        (
            PrioTable? prioTable,
            ITokenFactory<TParserResult>? tokenFactory,
            Func<TParserResult, IParserTokenType<TParserResult>>? converter,
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
            ITokenFactory<TParserResult>? tokenFactory,
            Func<TParserResult, IParserTokenType<TParserResult>>? converter,
            Component t
        )
            =>
                new(prioTable ?? PrioTable, tokenFactory ?? TokenFactory, converter ?? Converter, t);

        internal T? Get<T>()
        {
            Components.TryGetValue(typeof(T), out var result);
            return (T?)result;
        }

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

        ISubParser<TParserResult> CreateSubParser() => new SubParser<TParserResult>(Parser!, Converter!);

        IParser<TParserResult>? CreateParser()
        {
            if(PrioTable == null)
                return null;

            ITokenFactory<TParserResult> tokenFactory = new CachingTokenFactory<TParserResult>(TokenFactory!);
            var beginOfText = tokenFactory.BeginOfText;
            if(beginOfText == null)
                return null;

            return new PrioParser<TParserResult>
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
        ITokenFactory<TParserResult>? tokenFactory,
        Func<TParserResult, IParserTokenType<TParserResult>>? converter,
        object tag
    )
        => Dictionary[tag] =
            Dictionary.TryGetValue(tag, out var componentData)
                ? componentData.ReCreate(prioTable, tokenFactory, converter, this[tag])
                : new(prioTable, tokenFactory, converter, this[tag]);

    static string PrettyDumpPair(object key, ComponentData value) => key + "=" + ("\n" + value.PrettyDump).Indent();
}