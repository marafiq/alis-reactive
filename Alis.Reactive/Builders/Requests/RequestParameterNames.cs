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

    internal sealed class RequestScalarSlot
    {
        private readonly string _label;
        private readonly string _name;

        private RequestScalarSlot(string label, string name)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal static RequestScalarSlot Header(HeaderName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new RequestScalarSlot("header", name.Value);
        }

        internal static RequestScalarSlot RouteParameter(RouteParameterName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new RequestScalarSlot("route param", name.Value);
        }

        internal static RequestScalarSlot UrlQueryParameter(UrlParameterName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new RequestScalarSlot("URL param", name.Value);
        }

        internal Shape RequireShape<TProp>()
        {
            var shape = Shape.FromClrType(typeof(TProp));
            if (!shape.IsScalar)
                throw new InvalidOperationException(
                    $"{_label} '{_name}' requires a scalar type, but got shape '{shape.DescribeContract()}'. " +
                    "Use string, int, bool, DateTime, or their nullable variants.");

            return shape;
        }
    }
}
