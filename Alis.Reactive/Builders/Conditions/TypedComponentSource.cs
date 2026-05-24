using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads the current value of a component in the browser.
    /// Returned by each component's Value() extension method.
    /// </summary>
    /// <summary>A typed source that reads a property from a registered component.</summary>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly string _componentId;
        private readonly string _valueMember;

        internal TypedComponentSource(string componentId, string valueMember)
        {
            _componentId = componentId;
            _valueMember = valueMember;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.Read(PlanModel.ComponentSource.Of(_componentId), _valueMember, shape: Shape);

        internal override PlanModel.ComponentSource ToComponentSource() =>
            PlanModel.ComponentSource.Of(_componentId);

        internal string ComponentId => _componentId;
        internal override string ReadMember => _valueMember;
    }
}
