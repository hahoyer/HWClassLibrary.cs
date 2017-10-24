using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public class Compiler<TSourcePart> : DumpableObject
        where TSourcePart : class, ISourcePartProxy
    {
        sealed class ComponentData : DumpableObject
        {
            static ComponentData() => Tracer.Dumper.Configuration.Handlers.Add(typeof(Delegate), (type, o) => "?");

            readonly IDictionary<Type, object> Components =
                new Dictionary<Type, object>();

            readonly ValueCache<IParser<TSourcePart>> ParserCache;
            readonly ValueCache<ISubParser<TSourcePart>> SubParserCache;

            internal ComponentData
            (
                PrioTable prioTable,
                ITokenFactory<TSourcePart> tokenFactory,
                Func<TSourcePart, IParserTokenType<TSourcePart>> converter,
                Component component)
            {
                Add(prioTable, component);
                Add(tokenFactory, component);
                Add(converter, component);
                ParserCache = new ValueCache<IParser<TSourcePart>>(CreateParser);
                SubParserCache = new ValueCache<ISubParser<TSourcePart>>(CreateSubParser);
            }

            public string PrettyDump
                => Components
                    .Select(p => PrettyDumpPair(p.Key, p.Value))
                    .Stringify(separator: "\n");

            Func<TSourcePart, IParserTokenType<TSourcePart>> Converter =>
                Get<Func<TSourcePart, IParserTokenType<TSourcePart>>>();

            PrioTable PrioTable => Get<PrioTable>();
            ITokenFactory<TSourcePart> TokenFactory => Get<ITokenFactory<TSourcePart>>();

            internal IParser<TSourcePart> Parser => ParserCache.Value;
            internal ISubParser<TSourcePart> SubParser => SubParserCache.Value;

            string PrettyDumpPair(Type key, object value)
                => key.PrettyName() + "=" + ("\n" + PrettyDumpValue(value)).Indent();

            static string PrettyDumpValue(object value)
            {
                return Tracer.Dump(value);
            }

            ISubParser<TSourcePart> CreateSubParser() => new SubParser<TSourcePart>(Parser, Converter);

            IParser<TSourcePart> CreateParser()
            {
                if(PrioTable == null)
                    return null;

                ITokenFactory<TSourcePart> tokenFactory = new CachingTokenFactory<TSourcePart>(TokenFactory);
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

            internal ComponentData ReCreate
            (
                PrioTable prioTable,
                ITokenFactory<TSourcePart> tokenFactory,
                Func<TSourcePart, IParserTokenType<TSourcePart>> converter,
                Component t
            )
                =>
                    new ComponentData
                        (prioTable ?? PrioTable, tokenFactory ?? TokenFactory, converter ?? Converter, t);

            internal T Get<T>() => (T) Components[typeof(T)];

            internal void Add<T>(T value, Component parent)
            {
                Components.Add(typeof(T), value);

                if(value is IComponent component)
                    component.Current = parent;
            }
        }

        public interface IComponent
        {
            Component Current { set; }
        }

        public sealed class Component
        {
            public readonly Compiler<TSourcePart> Parent;
            public readonly object Tag;

            internal Component(Compiler<TSourcePart> parent, object tag)
            {
                Parent = parent;
                Tag = tag;
            }

            public PrioTable PrioTable { set => Parent.Define(value, null, null, Tag); }

            public ITokenFactory<TSourcePart> TokenFactory
            {
                set => Parent.Define(null, value, null, Tag);
            }

            public Func<TSourcePart, IParserTokenType<TSourcePart>> BoxFunction
            {
                set => Parent.Define(null, null, value, Tag);
            }

            public IParser<TSourcePart> Parser => Parent.Dictionary[Tag].Parser;
            public ISubParser<TSourcePart> SubParser => Parent.Dictionary[Tag].SubParser;
            public T Get<T>() => Parent.Dictionary[Tag].Get<T>();
            public void Add<T>(T value) { Parent.Dictionary[Tag].Add(value, this); }
        }

        readonly IDictionary<object, ComponentData> Dictionary =
            new Dictionary<object, ComponentData>();

        public Component this[object tag] => new Component(this, tag);

        public string PrettyDump
            => Dictionary
                .Select(p => PrettyDumpPair(p.Key, p.Value))
                .Stringify(separator: "\n");

        void Define
        (
            PrioTable prioTable,
            ITokenFactory<TSourcePart> tokenFactory,
            Func<TSourcePart, IParserTokenType<TSourcePart>> converter,
            object tag
        )
            => Dictionary[tag] =
                Dictionary.TryGetValue(tag, out var componentData)
                    ? componentData.ReCreate(prioTable, tokenFactory, converter, this[tag])
                    : new ComponentData(prioTable, tokenFactory, converter, this[tag]);

        string PrettyDumpPair(object key, ComponentData value) => key + "=" + ("\n" + value.PrettyDump).Indent();
    }
}