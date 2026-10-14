using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal abstract class PlanString : IEquatable<PlanString>
    {
        protected PlanString(string value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(parameterName + " must not be empty.", parameterName);

            Value = value;
        }

        internal string Value { get; }

        public bool Equals(PlanString? other) =>
            other != null && GetType() == other.GetType() && Value == other.Value;

        public override bool Equals(object? obj) => Equals(obj as PlanString);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((GetType().GetHashCode() * 397) ^ Value.GetHashCode());
            }
        }

        public override string ToString() => Value;
    }

    internal sealed class PlanId : PlanString
    {
        private PlanId(string value) : base(value, nameof(value)) { }

        internal static PlanId ForModel(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            return new PlanId(modelType.FullName
                ?? throw new ArgumentException("Model type must have a full name.", nameof(modelType)));
        }

        internal static PlanId Of(string value) => new PlanId(value);
    }

    internal sealed class ComponentId : PlanString
    {
        private ComponentId(string value) : base(value, nameof(value)) { }

        internal static ComponentId Of(string value) => new ComponentId(value);
    }

    internal sealed class BindingPath : PlanString
    {
        private readonly Path _path;

        private BindingPath(string value, Path path) : base(value, nameof(value))
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public Path Path => _path;

        // A developer-authored field name: its body path is read from the text.
        internal static BindingPath Of(string value) => new BindingPath(value, Path.Parse(value));

        // A model member's field: the MVC name text, and the body path read from the member's types.
        // Equality stays the name alone: one MVC name is one field.
        internal static BindingPath ForMember(string mvcName, Path bodyPath) => new BindingPath(mvcName, bodyPath);
    }

    internal sealed class MemberName : PlanString
    {
        private MemberName(string value) : base(value, nameof(value)) { }

        internal static MemberName Of(string value) => new MemberName(value);
    }

    internal sealed class ComponentKind : PlanString
    {
        private ComponentKind(string value) : base(value, nameof(value)) { }

        internal static ComponentKind Of(string value) => new ComponentKind(value);
    }

    internal sealed class EventName : PlanString
    {
        private EventName(string value) : base(value, nameof(value)) { }

        internal static EventName Of(string value) => new EventName(value);
    }

    internal sealed class PluginName : PlanString
    {
        private PluginName(string value) : base(value, nameof(value))
        {
            if (HasWhitespace(value))
                throw new ArgumentException("Plugin name must not contain whitespace.", nameof(value));
        }

        internal static PluginName Of(string value) => new PluginName(value);

        private static bool HasWhitespace(string value)
        {
            for (var characterIndex = 0; characterIndex < value.Length; characterIndex++)
            {
                if (char.IsWhiteSpace(value[characterIndex])) return true;
            }

            return false;
        }
    }

    internal sealed class HeaderName : PlanString
    {
        private HeaderName(string value) : base(value, nameof(value)) { }

        internal static HeaderName Of(string value) => new HeaderName(value);
    }

    internal sealed class RouteParameterName : PlanString
    {
        private static readonly System.Text.RegularExpressions.Regex Pattern =
            new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9_]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private RouteParameterName(string value) : base(value, nameof(value))
        {
            var hasInvalidCharacters = !Pattern.IsMatch(value);
            if (hasInvalidCharacters)
                throw new ArgumentException(
                    "Route param name '" + value + "' contains invalid characters. " +
                    "Names must match [a-zA-Z0-9_] (ASCII only) to align with URL template placeholders.",
                    nameof(value));
        }

        internal static RouteParameterName Of(string value) => new RouteParameterName(value);
    }

    internal sealed class HttpResponseStatusCode
    {
        private const int MinimumStandardStatusCode = 100;
        private const int MaximumStandardStatusCode = 599;

        private HttpResponseStatusCode(int value)
        {
            Value = value;
        }

        internal int Value { get; }

        internal static HttpResponseStatusCode FromDeveloperStatus(int value)
        {
            var isStandardHttpStatusCode =
                value >= MinimumStandardStatusCode &&
                value <= MaximumStandardStatusCode;
            if (isStandardHttpStatusCode) return new HttpResponseStatusCode(value);

            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "HTTP response status code must be between 100 and 599. " +
                "Use OnError(...) without a status code for network/client failures.");
        }
    }
}
