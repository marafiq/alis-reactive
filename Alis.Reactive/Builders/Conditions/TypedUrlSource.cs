using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a URL query parameter from the browser's current location.
    /// Returned by <c>PipelineBuilder.FromUrl()</c> and <c>PipelineBuilder.FromUrl&lt;T&gt;()</c>.
    /// Plugs into all TypedSource&lt;T&gt; consumers: conditions, guards, branches, element ops, gather, headers, route params.
    /// </summary>
    /// <summary>A typed source that reads a URL query parameter from the browser address bar.</summary>
    public sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            // URL params are single strings from URLSearchParams.get().
            // Reject non-scalar types — arrays, objects, complex types are not supported.
            var shape = Shape.FromClrType(typeof(TProp));
            if (!shape.IsScalar)
                throw new System.InvalidOperationException(
                    $"FromUrl<{typeof(TProp).Name}>(\"{paramName}\") is not supported. " +
                    "URL query parameters are single strings — use scalar types (string, int, bool, DateTime).");
            _paramName = urlParam.Value;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.ReadUrl(_paramName, shape: Shape);
    }
}
