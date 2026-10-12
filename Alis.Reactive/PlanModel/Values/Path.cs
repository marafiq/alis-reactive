using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{


    /// <summary>Property navigation segment: a property name or array index.</summary>
    internal sealed class PathSegment : IEquatable<PathSegment>
    {
        private readonly PathSegmentBody _body;

        /// <summary>JSON discriminator for path segments: <c>property</c> or <c>index</c>.</summary>
        public string Kind => _body.SegmentKind;

        private PathSegment(PathSegmentBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        internal static PathSegment Property(string name) =>
            new PathSegment(PathSegmentBody.Property(name));

        internal static PathSegment AtIndex(int index) =>
            new PathSegment(PathSegmentBody.Index(index));

        /// <summary>Each item of the list before it: <c>filesData[].rawFile</c> reads every item's file.</summary>
        internal static PathSegment EachItem { get; } = new PathSegment(PathSegmentBody.Each);

        internal bool IsEachItem => _body.SegmentKind == "each";

        public bool Equals(PathSegment? other) =>
            other != null
            && _body.HasSamePath(other._body);

        public override bool Equals(object? obj) => Equals(obj as PathSegment);

        public override int GetHashCode()
            => _body.GetPathHashCode();


        internal string ToPathText() => _body.PathText;
    }

    internal abstract class PathSegmentBody
    {
        private protected PathSegmentBody() { }

        internal abstract string SegmentKind { get; }
        internal abstract string PathText { get; }
        internal abstract bool HasSamePath(PathSegmentBody other);
        internal abstract int GetPathHashCode();

        internal static PathSegmentBody Property(string name) =>
            new PropertyPathSegmentBody(MemberName.Of(name));

        internal static PathSegmentBody Index(int index) =>
            new IndexPathSegmentBody(PathIndex.Of(index));

        internal static PathSegmentBody Each { get; } = new EachPathSegmentBody();

        private sealed class EachPathSegmentBody : PathSegmentBody
        {
            internal override string SegmentKind => "each";
            internal override string PathText => "[]";
            internal override bool HasSamePath(PathSegmentBody other) => other is EachPathSegmentBody;
            internal override int GetPathHashCode() => SegmentKind.GetHashCode();
        }

        private sealed class PropertyPathSegmentBody : PathSegmentBody
        {
            private readonly MemberName _name;

            internal PropertyPathSegmentBody(MemberName name)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
            }

            internal override string SegmentKind => "property";
            internal override string PathText => _name.Value;
            internal override bool HasSamePath(PathSegmentBody other) =>
                other is PropertyPathSegmentBody property && _name.Equals(property._name);
            internal override int GetPathHashCode()
            {
                unchecked
                {
                    return (SegmentKind.GetHashCode() * 397) ^ _name.GetHashCode();
                }
            }
        }

        private sealed class IndexPathSegmentBody : PathSegmentBody
        {
            private readonly PathIndex _index;

            internal IndexPathSegmentBody(PathIndex index)
            {
                _index = index;
            }

            internal override string SegmentKind => "index";
            internal override string PathText => _index.Text;
            internal override bool HasSamePath(PathSegmentBody other) =>
                other is IndexPathSegmentBody index && _index == index._index;
            internal override int GetPathHashCode()
            {
                unchecked
                {
                    return (SegmentKind.GetHashCode() * 397) ^ _index.GetHashCode();
                }
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

    /// <summary>Ordered path segments for navigating nested properties on a value.</summary>
    internal sealed class Path : IEquatable<Path>
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

        /// <summary>Ordered path segments used by runtime path traversal.</summary>
        public IReadOnlyList<PathSegment> Segments { get; }

        internal bool IsNone => Segments.Count == 0;

        private Path(IReadOnlyList<PathSegment> segments)
        {
            Segments = segments;
        }

        internal static Path Of(IEnumerable<PathSegment> segments) =>
            new Path(new List<PathSegment>(segments));

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

        internal Path Append(Path tail)
        {
            var list = new List<PathSegment>(Segments);
            list.AddRange(tail.Segments);
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

                var eachItem = part.EndsWith("[]", StringComparison.Ordinal);
                var name = eachItem ? part.Substring(0, part.Length - 2) : part;
                if (name.Length == 0)
                    segments.Add(PathSegment.EachItem);
                else if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIndex))
                    segments.Add(PathSegment.AtIndex(parsedIndex));
                else
                    segments.Add(PathSegment.Property(name));
                if (eachItem && name.Length != 0) segments.Add(PathSegment.EachItem);
            }

            // One [] reads a list member of each item; a second would read a list of lists, which no
            // shape the member declares can describe and form data cannot post.
            if (segments.Count(segment => segment.IsEachItem) > 1)
                throw new ArgumentException(
                    $"Path '{dotPath}' reads a list inside a list. Declare the inner list as its own member.",
                    nameof(dotPath));

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
            {
                if (segment.IsEachItem && parts.Count > 0) parts[parts.Count - 1] += "[]";
                else parts.Add(segment.ToPathText());
            }

            return string.Join(".", parts);
        }
    }
}
