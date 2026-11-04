using System;
using Alis.Reactive.Grammar;
using Alis.Reactive.Members;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanModel.Requests
{
    internal static class RequestScalarTarget
    {
        internal static Shape Header<TProp>(HeaderName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return RequireShape<TProp>("header", name.Value);
        }

        internal static void RouteParameter(string placeholder, Shape shape)
        {
            var undeclared = shape is AnyShape;
            if (undeclared || shape.IsScalar) return;

            throw new InvalidOperationException(
                $"Route placeholder '{{{placeholder}}}' needs a scalar value, but the member that fills it reads " +
                $"shape '{shape.DescribeContract()}'. Use string, a number, bool, a date, or their nullable forms.");
        }

        internal static Shape UrlQueryParameter<TProp>(string name)
        {
            var shape = ClrShape.Of(typeof(TProp));
            var isArrayOfScalars = shape.TryGetArrayItemShape(out var item) && item.IsScalar;
            return isArrayOfScalars ? shape : RequireShape<TProp>("URL param", name);
        }

        private static Shape RequireShape<TProp>(string label, string name)
        {
            var shape = ClrShape.Of(typeof(TProp));
            if (!shape.IsScalar)
            {
                throw new InvalidOperationException(
                    $"{label} '{name}' requires a scalar type, but got shape '{shape.DescribeContract()}'. " +
                    "Use string, int, bool, DateTime, or their nullable variants" + (label == "URL param" ? ", or an array of them." : "."));
            }

            return shape;
        }
    }
}
