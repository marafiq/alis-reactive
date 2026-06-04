using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source whose runtime value is read from the browser <c>window.location</c> query string.
    /// Returned by <c>PipelineBuilder.FromUrl()</c> and <c>PipelineBuilder.FromUrl&lt;T&gt;()</c>.
    /// Can be used anywhere a <see cref="TypedSource{TProp}"/> is accepted, including conditions and request inputs.
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
