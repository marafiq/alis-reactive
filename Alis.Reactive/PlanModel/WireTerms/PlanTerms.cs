using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal abstract class PlanString
    {
        // The term names what the developer gave ("Plugin name"), so a refusal says which name is missing.
        protected PlanString(string value, string term)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(term + " must not be empty.", nameof(value));

            Value = value;
        }

        internal string Value { get; }
    }

    internal sealed class PlanId : PlanString
    {
        private PlanId(string value) : base(value, "Plan id") { }

        internal static PlanId ForModel(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            return new PlanId(modelType.FullName
                ?? throw new ArgumentException("Model type must have a full name.", nameof(modelType)));
        }
    }

    internal sealed class ComponentId : PlanString
    {
        private ComponentId(string value) : base(value, "Component id") { }

        internal static ComponentId Of(string value) => new ComponentId(value);
    }

    internal sealed class BindingPath : PlanString
    {
        private readonly Path _path;

        private BindingPath(string value, Path path) : base(value, "Binding path")
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public Path Path => _path;

        // A model member's field: the MVC name text, and the body path read from the member's types.
        // Equality stays the name alone: one MVC name is one field.
        internal static BindingPath ForMember(string mvcName, Path bodyPath) => new BindingPath(mvcName, bodyPath);
    }

    internal sealed class MemberName : PlanString
    {
        private MemberName(string value) : base(value, "Member name") { }

        internal static MemberName Of(string value) => new MemberName(value);
    }

    internal sealed class EventName : PlanString
    {
        private EventName(string value) : base(value, "Event name") { }

        internal static EventName Of(string value) => new EventName(value);
    }

    internal sealed class PluginName : PlanString
    {
        private PluginName(string value) : base(value, "Plugin name")
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
        private HeaderName(string value) : base(value, "Header name") { }

        internal static HeaderName Of(string value) => new HeaderName(value);
    }

    internal sealed class RouteParameterName : PlanString
    {
        private static readonly System.Text.RegularExpressions.Regex Pattern =
            new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9_]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private RouteParameterName(string value) : base(value, "Route parameter name")
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
