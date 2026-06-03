using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class UrlParameterName
    {
        private UrlParameterName(string value)
        {
            Value = value;
        }

        internal string Value { get; }

        internal static UrlParameterName Of(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("URL param name must not be null or whitespace.", nameof(value));

            return new UrlParameterName(value);
        }
    }

    internal static class RequestScalarTarget
    {
        internal static Shape Header<TProp>(HeaderName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return RequireShape<TProp>("header", name.Value);
        }

        internal static Shape RouteParameter<TProp>(RouteParameterName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return RequireShape<TProp>("route param", name.Value);
        }

        internal static Shape UrlQueryParameter<TProp>(UrlParameterName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return RequireShape<TProp>("URL param", name.Value);
        }

        private static Shape RequireShape<TProp>(string label, string name)
        {
            var shape = Shape.FromClrType(typeof(TProp));
            if (!shape.IsScalar)
                throw new InvalidOperationException(
                    $"{label} '{name}' requires a scalar type, but got shape '{shape.DescribeContract()}'. " +
                    "Use string, int, bool, DateTime, or their nullable variants.");

            return shape;
        }
    }
}
