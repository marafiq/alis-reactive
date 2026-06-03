using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a URL query parameter from the browser's current location.
    /// Returned by <c>PipelineBuilder.FromUrl()</c> and <c>PipelineBuilder.FromUrl&lt;T&gt;()</c>.
    /// Plugs into all TypedSource&lt;T&gt; consumers: conditions, guards, branches, element ops, gather, headers, route params.
    /// </summary>
    public sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            RequestScalarTarget.UrlQueryParameter<TProp>(urlParam);
            _paramName = urlParam.Value;
        }

        internal override ValueExpression ToValueExpression() =>
            ValueExpression.ReadUrl(_paramName, shape: Shape);
    }
}
