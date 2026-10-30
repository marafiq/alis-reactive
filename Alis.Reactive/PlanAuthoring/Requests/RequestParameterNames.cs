using System;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal static class RequestScalarTarget
    {
        internal static Shape Header<TProp>(HeaderName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return RequireShape<TProp>("header", name.Value);
        }

        // A route value is one path segment: a scalar. A value whose shape is not declared (a plugin
        // result or a payload read typed as object) is the developer's to keep scalar.
        internal static void RouteParameter(string placeholder, Shape shape)
        {
            var undeclared = shape is NoneShape or AnyShape;
            if (undeclared || shape.IsScalar) return;

            throw new InvalidOperationException(
                $"Route placeholder '{{{placeholder}}}' needs a scalar value, but the member that fills it reads " +
                $"shape '{shape.DescribeContract()}'. Use string, a number, bool, a date, or their nullable forms.");
        }

        // A query key may repeat, and MVC binds an array from every value; a scalar takes the first.
        internal static Shape UrlQueryParameter<TProp>(string name)
        {
            var shape = Shape.FromClrType(typeof(TProp));
            var isArrayOfScalars = shape.TryGetArrayItemShape(out var item) && item.IsScalar;
            return isArrayOfScalars ? shape : RequireShape<TProp>("URL param", name);
        }

        private static Shape RequireShape<TProp>(string label, string name)
        {
            var shape = Shape.FromClrType(typeof(TProp));
            if (!shape.IsScalar)
                throw new InvalidOperationException(
                    $"{label} '{name}' requires a scalar type, but got shape '{shape.DescribeContract()}'. " +
                    "Use string, int, bool, DateTime, or their nullable variants" + (label == "URL param" ? ", or an array of them." : "."));

            return shape;
        }
    }
}
