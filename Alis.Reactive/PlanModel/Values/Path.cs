using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>One step of a path: a member by name, an item by position, or each item of a list.</summary>
    internal sealed class PathSegment : IEquatable<PathSegment>
    {
        private readonly string _text;
        private readonly int _index;

        private PathSegment(string kind, string text, int index)
        {
            Kind = kind;
            _text = text;
            _index = index;
        }

        internal string Kind { get; }

        internal static PathSegment Property(string name) => new PathSegment("property", MemberName.Of(name).Value, -1);

        internal static PathSegment AtIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Path index must not be negative.");
            return new PathSegment("index", index.ToString(CultureInfo.InvariantCulture), index);
        }

        internal static PathSegment EachItem { get; } = new PathSegment("each", "[]", -1);

        internal bool IsEachItem => Kind == "each";

        internal bool IsIndex => Kind == "index";

        /// <summary>The position an index step names.</summary>
        internal int Index => IsIndex
            ? _index
            : throw new InvalidOperationException($"'{_text}' names a member, not a position.");

        /// <summary>The step as the plan carries it: the member's name, the position's digits, or <c>[]</c> for each item.</summary>
        internal string ToPathText() => _text;

        public bool Equals(PathSegment? other) => other != null && Kind == other.Kind && _text == other._text;

        public override bool Equals(object? obj) => Equals(obj as PathSegment);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_text);
    }

    /// <summary>
    /// The one path model: from an object or a value, step by step, to a member. Every module builds a path
    /// from typed steps; the plan reads it as <see cref="Text"/>, a request body as <see cref="Body"/>.
    /// </summary>
    internal sealed class Path : IEquatable<Path>
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

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

        /// <summary>The path to each item of the list this path reads.</summary>
        internal Path Each()
        {
            var list = new List<PathSegment>(Segments) { PathSegment.EachItem };
            return new Path(list);
        }

        internal Path Append(Path tail)
        {
            var list = new List<PathSegment>(Segments);
            list.AddRange(tail.Segments);
            return new Path(list);
        }

        internal bool ReadsEachItem => Segments.Any(segment => segment.IsEachItem);

        /// <summary>The path as the plan carries it: one text per step; <c>[]</c> reads each item of a list.</summary>
        internal IReadOnlyList<string> Text => Segments.Select(segment => segment.ToPathText()).ToList();

        /// <summary>The path of a member that is written, called or listened to: the items of a list as a whole are none of those.</summary>
        internal IReadOnlyList<string> TargetText => ReadsEachItem
            ? throw new InvalidOperationException($"'{this}' reads each item of a list; it can be read, not written, called or subscribed to.")
            : Text;

        /// <summary>The path a JSON body is built along: a name makes an object, a position makes an array.</summary>
        internal IReadOnlyList<object> Body => Segments
            .Select(segment => segment.IsEachItem
                ? throw new InvalidOperationException($"'{this}' reads each item of a list; a body names members and positions.")
                : segment.IsIndex ? (object)segment.Index : segment.ToPathText())
            .ToList();

        /// <summary>
        /// Reads a path from text, at the boundaries that hand the framework text: an MVC field prefix and a
        /// vendor's object path. Everywhere else a path is built from typed steps.
        /// </summary>
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
