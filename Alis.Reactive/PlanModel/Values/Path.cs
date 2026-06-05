using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Serializes Path as a bare JSON array of PathSegments.
    /// Plan wire format: [{ "kind": "property", "name": "value" }]
    /// </summary>
    internal sealed class PathJsonConverter : JsonConverter<Path>
    {
        public override void Write(Utf8JsonWriter writer, Path value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var segment in value.Segments)
                segment.WriteJson(writer);
            writer.WriteEndArray();
        }

        public override Path Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }

    internal sealed class PathSegmentJsonConverter : JsonConverter<PathSegment>
    {
        public override void Write(Utf8JsonWriter writer, PathSegment value, JsonSerializerOptions options)
            => value.WriteJson(writer);

        public override PathSegment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>One step in a property navigation path: a property name or array index.</summary>
    [JsonConverter(typeof(PathSegmentJsonConverter))]
    public sealed class PathSegment : IEquatable<PathSegment>
    {
        private readonly PathSegmentBody _body;

        /// <summary>JSON discriminator for path segments: <c>property</c> or <c>index</c>.</summary>
        public string Kind => _body.KindForJson;

        private PathSegment(PathSegmentBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        internal static PathSegment Property(string name) =>
            new PathSegment(PathSegmentBody.Property(name));

        internal static PathSegment AtIndex(int index) =>
            new PathSegment(PathSegmentBody.Index(index));

        public bool Equals(PathSegment? other) =>
            other != null
            && _body.HasSamePath(other._body);

        public override bool Equals(object? obj) => Equals(obj as PathSegment);

        public override int GetHashCode()
            => _body.GetPathHashCode();

        internal void WriteJson(Utf8JsonWriter writer) => _body.WriteJson(writer);

        internal string ToPathText() => _body.PathText;
    }

    internal abstract class PathSegmentBody
    {
        private protected PathSegmentBody() { }

        internal abstract string KindForJson { get; }
        internal abstract string PathText { get; }
        internal abstract bool HasSamePath(PathSegmentBody other);
        internal abstract int GetPathHashCode();
        internal abstract void WriteJson(Utf8JsonWriter writer);

        internal static PathSegmentBody Property(string name) =>
            new PropertyPathSegmentBody(MemberName.Of(name));

        internal static PathSegmentBody Index(int index) =>
            new IndexPathSegmentBody(PathIndex.Of(index));

        private sealed class PropertyPathSegmentBody : PathSegmentBody
        {
            private readonly MemberName _name;

            internal PropertyPathSegmentBody(MemberName name)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
            }

            internal override string KindForJson => "property";
            internal override string PathText => _name.Value;
            internal override bool HasSamePath(PathSegmentBody other) =>
                other is PropertyPathSegmentBody property && _name.Equals(property._name);
            internal override int GetPathHashCode()
            {
                unchecked
                {
                    return (KindForJson.GetHashCode() * 397) ^ _name.GetHashCode();
                }
            }
            internal override void WriteJson(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString("kind", KindForJson);
                writer.WriteString("name", _name.Value);
                writer.WriteEndObject();
            }
        }

        private sealed class IndexPathSegmentBody : PathSegmentBody
        {
            private readonly PathIndex _index;

            internal IndexPathSegmentBody(PathIndex index)
            {
                _index = index;
            }

            internal override string KindForJson => "index";
            internal override string PathText => _index.Text;
            internal override bool HasSamePath(PathSegmentBody other) =>
                other is IndexPathSegmentBody index && _index == index._index;
            internal override int GetPathHashCode()
            {
                unchecked
                {
                    return (KindForJson.GetHashCode() * 397) ^ _index.GetHashCode();
                }
            }
            internal override void WriteJson(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString("kind", KindForJson);
                writer.WriteNumber("index", _index.Value);
                writer.WriteEndObject();
            }
        }
    }

    internal readonly struct PathIndex : IEquatable<PathIndex>
    {
        private PathIndex(int value)
        {
            Value = value;
        }

        internal int Value { get; }

        internal string Text => Value.ToString(CultureInfo.InvariantCulture);

        internal static PathIndex Of(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Path index must not be negative.");

            return new PathIndex(value);
        }

        public bool Equals(PathIndex other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is PathIndex other && Equals(other);

        public override int GetHashCode() => Value;

        public static bool operator ==(PathIndex left, PathIndex right) => left.Equals(right);

        public static bool operator !=(PathIndex left, PathIndex right) => !left.Equals(right);
    }

    /// <summary>An ordered sequence of segments for navigating nested properties on a value.</summary>
    [JsonConverter(typeof(PathJsonConverter))]
    public sealed class Path : IEquatable<Path>
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

        /// <summary>Ordered path segments used by runtime path traversal.</summary>
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

            var parts = dotPath.Split(new[] { '.' }, StringSplitOptions.None);
            var segments = new List<PathSegment>(parts.Length);
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                if (string.IsNullOrWhiteSpace(part))
                    throw new ArgumentException(
                        $"Path '{dotPath}' contains an empty segment at index {index}. " +
                        "Use dot-separated property names without consecutive, leading, or trailing dots.",
                        nameof(dotPath));

                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIndex))
                    segments.Add(PathSegment.AtIndex(parsedIndex));
                else
                    segments.Add(PathSegment.Property(part));
            }

            return new Path(segments);
        }

        public bool Equals(Path? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Segments.Count != other.Segments.Count) return false;

            for (var segmentIndex = 0; segmentIndex < Segments.Count; segmentIndex++)
            {
                if (!Segments[segmentIndex].Equals(other.Segments[segmentIndex])) return false;
            }

            return true;
        }

        internal bool Overlaps(Path other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return IsPrefixOf(other) || other.IsPrefixOf(this);
        }

        private bool IsPrefixOf(Path other)
        {
            if (Segments.Count > other.Segments.Count) return false;

            for (var segmentIndex = 0; segmentIndex < Segments.Count; segmentIndex++)
            {
                if (!Segments[segmentIndex].Equals(other.Segments[segmentIndex])) return false;
            }

            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as Path);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                foreach (var segment in Segments)
                    hash = (hash * 31) + segment.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            if (Segments.Count == 0) return string.Empty;

            var parts = new List<string>(Segments.Count);
            foreach (var segment in Segments)
                parts.Add(segment.ToPathText());

            return string.Join(".", parts);
        }
    }
}
