using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using hw.Tests.CompilerTool.Util;

namespace hw.Parser
{
    public class Compiler<TTreeItem> : DumpableObject
        where TTreeItem : class, ISourcePart
    {
        sealed class ComponentData : DumpableObject
        {
            readonly IDictionary<System.Type, object> Components =
                new Dictionary<System.Type, object>();

            readonly PrioTable PrioTable;
            readonly ITokenFactory TokenFactory;

            readonly ValueCache<IParser<TTreeItem>> ParserCache;
            readonly ValueCache<ISubParser<TTreeItem>> SubParserCache;
            readonly Func<TTreeItem, IParserTokenType<TTreeItem>> Converter;

            internal IParser<TTreeItem> Parser => ParserCache.Value;
            internal ISubParser<TTreeItem> SubParser => SubParserCache.Value;

            internal ComponentData
            (
                PrioTable prioTable,
                ITokenFactory tokenFactory,
                Func<TTreeItem, IParserTokenType<TTreeItem>> converter)
            {
                PrioTable = prioTable;
                TokenFactory = tokenFactory;
                Converter = converter;
                ParserCache = new ValueCache<IParser<TTreeItem>>(CreateParser);
                SubParserCache = new ValueCache<ISubParser<TTreeItem>>(CreateSubParser);
            }

            ISubParser<TTreeItem> CreateSubParser() => new SubParser<TTreeItem>(Parser, Converter);

            IParser<TTreeItem> CreateParser()
            {
                if (PrioTable == null)
                    return null;

                ITokenFactory tokenFactory = new CachingTokenFactory(TokenFactory);
                var beginOfText = tokenFactory.BeginOfText as IParserTokenType<TTreeItem>;
                if (beginOfText == null)
                    return null;

                return new PrioParser<TTreeItem>
                (
                    PrioTable,
                    new TwoLayerScanner(tokenFactory),
                    beginOfText
                );
            }

            internal ComponentData ReCreate
                (
                    PrioTable prioTable,
                    ITokenFactory tokenFactory,
                    Func<TTreeItem, IParserTokenType<TTreeItem>> converter
                )
                =>
                new ComponentData
                    (prioTable ?? PrioTable, tokenFactory ?? TokenFactory, converter ?? Converter);

            internal T Get<T>() => (T)Components[typeof(T)];
            internal void Add<T>(T value) => Components.Add(typeof(T), value);
        }

        public interface IComponent
        {
            Component Current { set; }
        }

        readonly IDictionary<object, ComponentData> Dictionary =
            new Dictionary<object, ComponentData>();
        readonly object EmptyTag = new object();

        internal Component this[object tag] => new Component(this, tag);

        public sealed class Component
        {
            public readonly Compiler<TTreeItem> Parent;
            public readonly object Tag;

            internal Component(Compiler<TTreeItem> parent, object tag)
            {
                Parent = parent;
                Tag = tag;
            }

            public PrioTable PrioTable { set { Parent.Define(value, null, null, Tag); } }

            public ITokenFactory TokenFactory
            {
                set
                {
                    Parent.Define(null, value, null, Tag);
                    var component = value as IComponent;
                    if (component != null)
                        component.Current = this;
                }
            }

            public Func<TTreeItem, IParserTokenType<TTreeItem>> BoxFunction
            {
                set { Parent.Define(null, null, value, Tag); }
            }

            public IParser<TTreeItem> Parser => Parent.Dictionary[Tag].Parser;
            public ISubParser<TTreeItem> SubParser => Parent.Dictionary[Tag].SubParser;
            public T Get<T>() => Parent.Dictionary[Tag].Get<T>();

            public void Add<T>(T value)
            {
                Parent.Dictionary[Tag].Add(value);
                var component = value as IComponent;
                if (component != null)
                    component.Current = this;
            }
        }

        void Define
        (
            PrioTable prioTable,
            ITokenFactory tokenFactory,
            Func<TTreeItem, IParserTokenType<TTreeItem>> converter,
            object tag
        )
        {
            ComponentData componentData;
            Dictionary[tag] =
                Dictionary.TryGetValue(tag, out componentData)
                    ? componentData.ReCreate(prioTable, tokenFactory, converter)
                    : new ComponentData(prioTable, tokenFactory, converter);
        }

        public PrioTable PrioTable { set { Define(value, null, null, EmptyTag); } }
        public ITokenFactory TokenFactory { set { Define(null, value, null, EmptyTag); } }
        public IParser<TTreeItem> Parser => Dictionary[EmptyTag].Parser;
        public T Get<T>() => Dictionary[EmptyTag].Get<T>();
        public void Add<T>(T value) => Dictionary[EmptyTag].Add(value);
    }
}