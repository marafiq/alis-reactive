using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for all value nodes in a reactive plan. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueProducer>))]
    public abstract class ValueProducer
    {
        private protected ValueProducer() { }

        internal static ValueProducer Literal(bool value) =>
            new LiteralProducer(value, Shape.Boolean);

        internal static ValueProducer Literal(string value) =>
            new LiteralProducer(value, Shape.String);

        internal static ValueProducer Literal(int value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(long value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(decimal value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(double value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(DateTime value) =>
            new LiteralProducer(value.ToString("O"), Shape.Date);

        internal static ValueProducer Null() =>
            new LiteralProducer(null, Shape.None);

        internal static readonly ValueProducer None = new NoneProducer();

        internal bool IsNone => this is NoneProducer;

        /// <summary>
        /// Creates a literal with any JSON-serializable value.
        /// System.Text.Json handles serialization at Render time.
        /// No reflection. No type inspection. The serializer does the work.
        /// </summary>
        internal static ValueProducer LiteralRaw(object value, Shape shape) =>
            new LiteralProducer(value, shape);

        /// <summary>Creates a ReadProducer that reads a URL query parameter by name.
        /// Default shape is String because URL params are inherently strings.</summary>
        internal static ValueProducer ReadUrl(string paramName, Shape shape = null) =>
            Read(UrlSource.Instance, paramName, shape: shape ?? Shape.String);

        internal static ValueProducer Read(Source from, string member, Path path = null, Shape shape = null, List<ValueProducer> args = null) =>
            new ReadProducer(from, member, path, shape, args);

        internal static ValueProducer Object(Dictionary<string, ValueProducer> fields, Shape shape = null) =>
            new ObjectProducer(fields, shape);

        internal static ValueProducer Array(List<ValueProducer> items, Shape shape = null) =>
            new ArrayProducer(items, shape);
    }

    /// <summary>A constant value embedded in the plan.</summary>
    /// <remarks>
    /// Created when a literal is passed to a builder such as <c>p.Element("id").SetText("hello")</c>.
    /// </remarks>
    public sealed class LiteralProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"literal"</c>.</summary>
        public string Kind => "literal";
        /// <summary>Gets the constant value embedded in the plan.</summary>
        [JsonInclude]
        public object Value { get; }
        /// <summary>Gets the expected type shape, or <see langword="null"/> when not specified.</summary>
        public Shape Shape { get; }

        internal LiteralProducer(object value, Shape shape)
        {
            Value = value;
            Shape = shape == Shape.None ? null : shape;
        }
    }

    /// <summary>A value read from a live source when the plan executes in the browser.</summary>
    /// <remarks>
    /// Created by source-reading builders such as <c>p.Plugin&lt;int&gt;("array", "count").Arg(json, x =&gt; x.Items)</c>.
    /// </remarks>
    public sealed class ReadProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"read"</c>.</summary>
        public string Kind => "read";
        /// <summary>
        /// Gets the value source: <see cref="ComponentSource"/>, <see cref="PluginSource"/>,
        /// <see cref="UrlSource"/>, or <see cref="PayloadSource"/>.
        /// </summary>
        public Source From { get; }
        /// <summary>Gets the property or method name to read on the source.</summary>
        public string Member { get; }
        /// <summary>Gets the nested property path, or <see langword="null"/> for direct reads.</summary>
        public Path Path { get; }
        /// <summary>Gets the expected type shape, or <see langword="null"/> when not specified.</summary>
        public Shape Shape { get; }
        /// <summary>Gets optional method arguments, or <see langword="null"/> for property reads.</summary>
        public IReadOnlyList<ValueProducer> Args { get; }

        internal ReadProducer(Source from, string member, Path path, Shape shape, List<ValueProducer> args = null)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Path = path == null || path.IsNone ? null : path;
            Shape = shape == null || shape.IsNone ? null : shape;
            Args = args != null && args.Count > 0 ? args : null;
        }
    }

    /// <summary>A composite value built from named field expressions.</summary>
    public sealed class ObjectProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"object"</c>.</summary>
        public string Kind => "object";
        /// <summary>Gets the named fields and their value expressions.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Fields { get; }
        /// <summary>Gets the expected type shape, or <see langword="null"/> when not specified.</summary>
        public Shape Shape { get; }

        internal ObjectProducer(Dictionary<string, ValueProducer> fields, Shape shape)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
            Shape = shape == null || shape.IsNone ? null : shape;
        }

        /// <summary>Gets the underlying mutable dictionary for building nested structures.</summary>
        internal Dictionary<string, ValueProducer> WritableFields => (Dictionary<string, ValueProducer>)Fields;
    }

    /// <summary>A composite value built from ordered item expressions.</summary>
    public sealed class ArrayProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"array"</c>.</summary>
        public string Kind => "array";
        /// <summary>Gets the ordered item expressions.</summary>
        public IReadOnlyList<ValueProducer> Items { get; }
        /// <summary>Gets the expected type shape, or <see langword="null"/> when not specified.</summary>
        public Shape Shape { get; }

        internal ArrayProducer(List<ValueProducer> items, Shape shape)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape == null || shape.IsNone ? null : shape;
        }
    }

    /// <summary>Sentinel for "no value specified." Not constructed in application code.</summary>
    public sealed class NoneProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";

        internal NoneProducer() { }
    }
}
