using Alis.Reactive.PlanModel.Values;

namespace Alis.Reactive.PlanAuthoring.Pipelines
{
    /// <summary>
    /// A member of the view's model as MVC binds it, read when the reaction runs, by its binding name, from the input
    /// that binds it, whichever view rendered it; missing when none is on the page. See <see cref="PipelineBuilder{TModel}.Bound{TProp}"/>.
    /// </summary>
    internal sealed class BoundValue<TProp> : TypedSource<TProp>
    {
        private readonly string _name;

        internal BoundValue(string name)
        {
            _name = name;
        }

        internal override ValueExpression Expression => ValueExpression.ReadBound(_name, Shape);
    }
}
