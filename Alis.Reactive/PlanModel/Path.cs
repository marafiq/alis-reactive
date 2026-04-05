using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PathSegment
    {
        public string Kind { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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

    internal sealed class Path
    {
        internal static readonly Path None = new Path(Array.Empty<PathSegment>());

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
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                    segments.Add(PathSegment.AtIndex(index));
                else
                    segments.Add(PathSegment.Property(part));
            }

            return new Path(segments);
        }
    }
}
