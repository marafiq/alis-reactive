using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>One step of a path: a member by name, an item by position, or each item of a list.</summary>
    internal sealed class PathSegment
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
    }

    /// <summary>
    /// The one path model: from an object or a value, step by step, to a member. Every module builds a path
    /// from typed steps; the plan reads it as <see cref="Text"/>, a request body as <see cref="Body"/>.
    /// </summary>
    internal sealed class Path
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
        /// Reads a path from text, at the boundaries that hand the framework text: an MVC field prefix
        /// (<c>NextOfKin</c>, <c>Members[1].Address</c>) and a vendor's object path (<c>slate</c>,
        /// <c>ej2_instances.0</c>). A step of digits, or digits in brackets as MVC names a list item, is a
        /// position; any other step is a member. Everywhere else a path is built from typed steps.
        /// </summary>
        internal static Path Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return None;

            var steps = text.Replace("]", "").Replace('[', '.').Split('.');
            var segments = new List<PathSegment>(steps.Length);
            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                if (string.IsNullOrWhiteSpace(step))
                    throw new ArgumentException(
                        $"Path '{text}' contains an empty segment at index {index}. " +
                        "Use dot-separated property names without consecutive, leading, or trailing dots.",
                        nameof(text));

                segments.Add(int.TryParse(step, NumberStyles.None, CultureInfo.InvariantCulture, out var position)
                    ? PathSegment.AtIndex(position)
                    : PathSegment.Property(step));
            }

            return new Path(segments);
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
