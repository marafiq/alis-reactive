using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class JsType
    {
        private readonly Dictionary<string, JsProperty> _properties = new Dictionary<string, JsProperty>();
        private readonly Dictionary<string, JsMethod> _methods = new Dictionary<string, JsMethod>();
        private readonly Dictionary<string, JsEvent> _events = new Dictionary<string, JsEvent>();

        public IReadOnlyDictionary<string, JsProperty> Properties => _properties;
        public IReadOnlyDictionary<string, JsMethod> Methods => _methods;
        public IReadOnlyDictionary<string, JsEvent> Events => _events;

        internal JsType() { }

        internal JsType WithProperty(string name, Path path, Shape shape, string access)
        {
            if (_properties.TryGetValue(name, out var existing))
            {
                // Keep the more specific shape: typed shape wins over Any/None/Nullable wrapper.
                // Compatible pairs: Date ↔ Nullable(Date), String ↔ Nullable(String), etc.
                var keepShape = ShapeCompat.Resolve(existing.Shape, shape);
                if (keepShape == null)
                    throw new System.InvalidOperationException(
                        $"Property '{name}' registered with shape '{existing.Shape.Kind}' " +
                        $"but re-registered with conflicting shape '{shape.Kind}'.");
                // Widen access: read + write → readwrite. Explicit cases only.
                var widenedAccess = (existing.Access, access) switch
                {
                    ("readwrite", _) => "readwrite",
                    (_, "readwrite") => "readwrite",
                    ("read", "write") => "readwrite",
                    ("write", "read") => "readwrite",
                    ("read", "read") => "read",
                    ("write", "write") => "write",
                    _ => throw new System.InvalidOperationException(
                        $"Property '{name}' has unknown access pair: '{existing.Access}' + '{access}'."),
                };
                // Keep the existing path — the first registration defines the resolution path.
                // Later re-registrations (from conditions, mutations) use the same member name
                // and their paths are expected to match.
                _properties[name] = new JsProperty(existing.Path, keepShape, widenedAccess);
            }
            else
            {
                _properties[name] = new JsProperty(path, shape, access);
            }
            return this;
        }

        internal JsType WithMethod(string name, Path path, List<Shape>? args = null, Shape? returns = null)
        {
            _methods[name] = new JsMethod(path, args, returns);
            return this;
        }

        internal JsType WithEvent(string name, string channel, string? payloadType = null)
        {
            _events[name] = new JsEvent(channel, payloadType);
            return this;
        }

    }

    /// <summary>
    /// Picks the most specific compatible shape from two registrations.
    /// Returns null if incompatible (true conflict).
    /// </summary>
    internal static class ShapeCompat
    {
        internal static Shape Resolve(Shape a, Shape b)
        {
            if (a == b) return a;
            // Any/None are wildcards — the other wins
            if (a == Shape.Any || a == Shape.None) return b;
            if (b == Shape.Any || b == Shape.None) return a;
            // Nullable wrapping: Date ↔ Nullable(Date) → keep Nullable(Date)
            if (a.Kind == "nullable" && a.Inner == b) return a;
            if (b.Kind == "nullable" && b.Inner == a) return b;
            return null; // incompatible
        }
    }

    internal sealed class JsProperty
    {
        public Path Path { get; }
        public Shape Shape { get; }
        public string Access { get; }

        internal JsProperty(Path path, Shape shape, string access)
        {
            Path = path;
            Shape = shape;
            Access = access;
        }
    }

    internal sealed class JsMethod
    {
        public Path Path { get; }
        public System.Collections.Generic.IReadOnlyList<Shape> Args { get; }
        public Shape Returns { get; }

        internal JsMethod(Path path, List<Shape> args = null, Shape returns = null)
        {
            Path = path;
            Args = args != null && args.Count > 0 ? args : (System.Collections.Generic.IReadOnlyList<Shape>)System.Array.Empty<Shape>();
            Returns = returns ?? Shape.None;
        }
    }

    internal sealed class JsEvent
    {
        public string Channel { get; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? PayloadType { get; }

        internal JsEvent(string channel, string? payloadType)
        {
            Channel = channel;
            PayloadType = payloadType;
        }
    }

}
