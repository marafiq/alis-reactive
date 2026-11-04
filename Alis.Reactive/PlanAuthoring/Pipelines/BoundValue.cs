using Alis.Reactive.PlanModel.Values;

namespace Alis.Reactive.PlanAuthoring.Pipelines
{
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
