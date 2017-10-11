using System;
using System.Collections.Generic;
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

            Func<TSourcePart, IParserTokenType<TSourcePart>> Converter =>
                Get<Func<TSourcePart, IParserTokenType<TSourcePart>>>();

            PrioTable PrioTable => Get<PrioTable>();
            ITokenFactory<TSourcePart> TokenFactory => Get<ITokenFactory<TSourcePart>>();

            internal IParser<TSourcePart> Parser => ParserCache.Value;
            internal ISubParser<TSourcePart> SubParser => SubParserCache.Value;

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

                var component = value as IComponent;
                if(component != null)
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

            public PrioTable PrioTable { set { Parent.Define(value, tokenFactory: null, converter: null, tag: Tag); } }

            public ITokenFactory<TSourcePart> TokenFactory
            {
                set { Parent.Define(prioTable: null, tokenFactory: value, converter: null, tag: Tag); }
            }

            public Func<TSourcePart, IParserTokenType<TSourcePart>> BoxFunction
            {
                set { Parent.Define(prioTable: null, tokenFactory: null, converter: value, tag: Tag); }
            }

            public IParser<TSourcePart> Parser => Parent.Dictionary[Tag].Parser;
            public ISubParser<TSourcePart> SubParser => Parent.Dictionary[Tag].SubParser;
            public T Get<T>() => Parent.Dictionary[Tag].Get<T>();
            public void Add<T>(T value) { Parent.Dictionary[Tag].Add(value, this); }
        }

        readonly IDictionary<object, ComponentData> Dictionary =
            new Dictionary<object, ComponentData>();

        readonly object EmptyTag = new object();

        public Component this[object tag] => new Component(this, tag);

        public PrioTable PrioTable { set { Define(value, tokenFactory: null, converter: null, tag: EmptyTag); } }

        public IParser<TSourcePart> Parser => Dictionary[EmptyTag].Parser;

        public ITokenFactory<TSourcePart> TokenFactory
        {
            set { Define(prioTable: null, tokenFactory: value, converter: null, tag: EmptyTag); }
        }

        void Define
        (
            PrioTable prioTable,
            ITokenFactory<TSourcePart> tokenFactory,
            Func<TSourcePart, IParserTokenType<TSourcePart>> converter,
            object tag
        )
        {
            ComponentData componentData;
            Dictionary[tag] =
                Dictionary.TryGetValue(tag, out componentData)
                    ? componentData.ReCreate(prioTable, tokenFactory, converter, this[EmptyTag])
                    : new ComponentData(prioTable, tokenFactory, converter, this[EmptyTag]);
        }

        public T Get<T>() => Dictionary[EmptyTag].Get<T>();
        public void Add<T>(T value) => Dictionary[EmptyTag].Add(value, this[EmptyTag]);
    }
}