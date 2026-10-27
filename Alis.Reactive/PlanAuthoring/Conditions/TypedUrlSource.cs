using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>A key of the page's URL query string, read when the reaction runs; see <see cref="UrlQuery{TQuery}"/>.</summary>
    internal sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            RequestScalarTarget.UrlQueryParameter<TProp>(urlParam);
            _paramName = urlParam.Value;
        }

        internal override ValueExpression Expression =>
            ValueExpression.ReadUrl(_paramName, shape: Shape);
    }
}
