using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A member of the view's model as MVC binds it, read when the reaction runs from the input rendered for it,
    /// whichever view rendered it; missing when none is on the page. See <see cref="PipelineBuilder{TModel}.Bound{TProp}"/>.
    /// </summary>
    internal sealed class BoundValue<TProp> : TypedSource<TProp>
    {
        private readonly string _inputId;

        internal BoundValue(string inputId)
        {
            _inputId = inputId;
        }

        internal override ValueExpression Expression => ValueExpression.ReadBound(_inputId, Shape);
    }
}
