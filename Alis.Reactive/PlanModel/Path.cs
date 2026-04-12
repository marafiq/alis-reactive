using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Serializes Path as a bare JSON array of PathSegments.
    /// Schema expects: [{ "kind": "property", "name": "value" }]
    /// </summary>
    internal sealed class PathJsonConverter : JsonConverter<Path>
    {
        public override void Write(Utf8JsonWriter writer, Path value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var segment in value.Segments)
                JsonSerializer.Serialize(writer, segment, options);
            writer.WriteEndArray();
        }

        public override Path Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>One step in a property navigation path: a property name or array index.</summary>
    public sealed class PathSegment
    {
        /// <summary>Gets the segment kind (property or index).</summary>
        public string Kind { get; }

        /// <summary>Gets the property name for property segments, or <see langword="null"/> for index segments.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; }

        /// <summary>Gets the array index for index segments, or <see langword="null"/> for property segments.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Index { get; }

        private PathSegment(string kind, string name, int? index)
        {
            Kind = kind;
            Name = name;
            Index = index;
        }

        internal static PathSegment Property(string name) =>
            new PathSegment("property", name, null);

        internal static PathSegment AtIndex(int index) =>
            new PathSegment("index", null, index);
    }

    /// <summary>An ordered sequence of segments for navigating nested properties on a value.</summary>
    [JsonConverter(typeof(PathJsonConverter))]
    public sealed class Path
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

        /// <summary>Gets the ordered path segments.</summary>
        [JsonIgnore]
        public IReadOnlyList<PathSegment> Segments { get; }

        internal bool IsNone => Segments.Count == 0;

        private Path(IReadOnlyList<PathSegment> segments)
        {
            Segments = segments;
        }

        internal static Path Property(string name) =>
            new Path(new[] { PathSegment.Property(name) });

        internal Path Then(string name)
        {
            var list = new List<PathSegment>(Segments) { PathSegment.Property(name) };
            return new Path(list);
        }

        internal Path AtIndex(int index)
        {
            var list = new List<PathSegment>(Segments) { PathSegment.AtIndex(index) };
            return new Path(list);
        }

        internal static Path Parse(string dotPath)
        {
            if (string.IsNullOrEmpty(dotPath)) return None;

            var parts = dotPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var segments = new List<PathSegment>(parts.Length);
            foreach (var part in parts)
            {
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx))
                    segments.Add(PathSegment.AtIndex(idx));
                else
                    segments.Add(PathSegment.Property(part));
            }

            return new Path(segments);
        }
    }
}
