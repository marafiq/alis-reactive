using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    internal abstract class CapabilityMember
    {
        protected CapabilityMember(string name, IReadOnlyList<PathSegment> path)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Capability member name cannot be empty.", nameof(name));

            Name = name;
            Path = CapabilityPath.Clone(path);
        }

        internal string Name { get; }
        internal List<PathSegment> Path { get; }
    }

    internal sealed class CapabilityProperty : CapabilityMember
    {
        private CapabilityProperty(string name, IReadOnlyList<PathSegment> path)
            : base(name, path)
        {
        }

        internal static CapabilityProperty Named(string name) =>
            new CapabilityProperty(name, new[] { PathSegment.FromProp(name) });

        internal static CapabilityProperty FromSegments(string name, IReadOnlyList<PathSegment> path) =>
            new CapabilityProperty(name, path);
    }

    internal sealed class CapabilityMethod : CapabilityMember
    {
        private CapabilityMethod(string name, IReadOnlyList<PathSegment> path)
            : base(name, path)
        {
        }

        internal static CapabilityMethod Named(string name) =>
            new CapabilityMethod(name, new[] { PathSegment.FromProp(name) });

        internal static CapabilityMethod FromSegments(string name, IReadOnlyList<PathSegment> path) =>
            new CapabilityMethod(name, path);
    }

    internal static class CapabilityPath
    {
        internal static List<PathSegment> Clone(IReadOnlyList<PathSegment> path)
        {
            var segments = new List<PathSegment>(path.Count);
            foreach (var segment in path)
            {
                if (segment.Prop != null)
                {
                    segments.Add(PathSegment.FromProp(segment.Prop));
                    continue;
                }

                if (segment.Index.HasValue)
                    segments.Add(PathSegment.FromIndex(segment.Index.Value));
            }

            return segments;
        }

        internal static bool Same(IReadOnlyList<PathSegment> left, IReadOnlyList<PathSegment> right)
        {
            if (left.Count != right.Count)
                return false;

            return !left.Where((t, i) =>
                    !string.Equals(t.Prop, right[i].Prop, StringComparison.Ordinal)
                    || t.Index != right[i].Index)
                .Any();
        }

        internal static string Format(IReadOnlyList<PathSegment> path)
        {
            if (path.Count == 0)
                return string.Empty;

            var segments = new List<string>(path.Count);
            foreach (var segment in path)
            {
                if (segment.Prop != null)
                {
                    segments.Add(segment.Prop);
                    continue;
                }

                if (segment.Index.HasValue)
                    segments.Add(segment.Index.Value.ToString(CultureInfo.InvariantCulture));
            }

            return string.Join(".", segments);
        }
    }
}
