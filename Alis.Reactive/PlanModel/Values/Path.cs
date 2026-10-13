using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>One step of a path: a property by name, an index into a list, or each item of the list before it.</summary>
    internal sealed class PathSegment : IEquatable<PathSegment>
    {
        private readonly string _text;

        private PathSegment(string kind, string text)
        {
            Kind = kind;
            _text = text;
        }

        /// <summary><c>property</c>, <c>index</c> or <c>each</c>.</summary>
        internal string Kind { get; }

        internal static PathSegment Property(string name) => new PathSegment("property", MemberName.Of(name).Value);

        internal static PathSegment AtIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Path index must not be negative.");
            return new PathSegment("index", index.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Each item of the list before it: <c>filesData[].rawFile</c> reads every item's file.</summary>
        internal static PathSegment EachItem { get; } = new PathSegment("each", "[]");

        internal bool IsEachItem => Kind == "each";

        internal string ToPathText() => _text;

        public bool Equals(PathSegment? other) => other != null && Kind == other.Kind && _text == other._text;

        public override bool Equals(object? obj) => Equals(obj as PathSegment);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_text);
    }

    /// <summary>Ordered path segments for navigating nested properties on a value.</summary>
    internal sealed class Path : IEquatable<Path>
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

        /// <summary>Ordered path segments used by runtime path traversal.</summary>
        internal IReadOnlyList<PathSegment> Segments { get; }

        internal bool IsNone => Segments.Count == 0;

        private Path(IReadOnlyList<PathSegment> segments)
        {
            Segments = segments;
        }

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

        public bool Equals(Path? other) => other != null && Segments.SequenceEqual(other.Segments);

        public override bool Equals(object? obj) => Equals(obj as Path);

        public override int GetHashCode() =>
            Segments.Aggregate(17, (hash, segment) => unchecked(hash * 31 + segment.GetHashCode()));

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
